using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuManagement
{
    public class MonitoringQueueOnDisk
    {
        public Queue<MonitoringPortOnDisk> Queue { get; set; } = new Queue<MonitoringPortOnDisk>();

        public MonitoringQueueOnDisk(Queue<MonitoringPort> queue)
        {
            foreach (var p in queue)
            {
                Queue.Enqueue(new MonitoringPortOnDisk(p));
            }
        }

        public void Save(string monitoringSettingsFile)
        {
            try
            {
                var json = JsonConvert.SerializeObject(Queue);
                File.WriteAllText(monitoringSettingsFile, json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public class MonitoringQueue
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly IMyLog _logFile;
        private readonly string _monitoringSettingsFile = Utils.FileNameForSure(@"..\ini\", @"monitoring.que", false);
        private readonly string _monitoringSettingFileBackup = Utils.FileNameForSure(@"..\ini\", @"monitoring.que.bac", false);
        private readonly string _monitoringSettingsMd5File = Utils.FileNameForSure(@"..\ini\", @"monitoring.que.md5", false);
        public Queue<MonitoringPort> Queue { get; set; }

        public MonitoringQueue(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public int Count() { return Queue.Count; }

        public MonitoringPort Peek() { return Queue.Peek(); }
        public MonitoringPort Dequeue() { return Queue.Dequeue(); }
        public void Enqueue(MonitoringPort item) { Queue.Enqueue(item); }

        private string[] LoadWithMd5()
        {
            try
            {
                if (File.Exists(_monitoringSettingsFile))
                {
                    if (File.Exists(_monitoringSettingsMd5File))
                    {
                        var md5 = CalculateMd5(_monitoringSettingsFile);
                        var md5FromFile = File.ReadAllText(_monitoringSettingsMd5File);
                        return File.ReadAllLines(md5 == md5FromFile ? _monitoringSettingsFile : _monitoringSettingFileBackup);
                    }
                }
                else if (File.Exists(_monitoringSettingFileBackup))
                {
                    return File.ReadAllLines(_monitoringSettingFileBackup);
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Queue loading: {e.Message}");
            }

            return new string[0];
        }

        public void Clear()
        {
            Queue.Clear();
            Save();
        }

        public void ClearRtuStatusEvents()
        {
            var list = Queue.ToList();
            Queue.Clear();

            foreach (var monitoringPort in list)
            {
                monitoringPort.LastMoniResult.UserReturnCode = ReturnCode.MeasurementEndedNormally;
                Queue.Enqueue(monitoringPort);
            }

            Save();
        }

        public void Load()
        {
            _logFile.EmptyLine();
            _logFile.AppendLine("Monitoring queue assembling...");
            Queue = new Queue<MonitoringPort>();

            try
            {

                var contents = LoadWithMd5();
                var list = contents.Select(s => (MonitoringPortOnDisk)JsonConvert.DeserializeObject(s, JsonSerializerSettings)).ToList();

                foreach (var port in list)
                {
                    Queue.Enqueue(new MonitoringPort(port));
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Queue parsing: {e.Message}");
            }

            _logFile.AppendLine($"{Queue.Count} port(s) in queue.");
        }

        public void Save()
        {
            try
            {
                var list = Queue.Select(p => JsonConvert.SerializeObject(new MonitoringPortOnDisk(p), JsonSerializerSettings)).ToList();
                File.WriteAllLines(_monitoringSettingsFile, list);

                // save json with the one and only root (just for viewers)
                var queueOnDisk = new MonitoringQueueOnDisk(Queue);
                queueOnDisk.Save(_monitoringSettingsFile + ".json");

                var md5 = CalculateMd5(_monitoringSettingsFile);
                File.WriteAllText(_monitoringSettingsMd5File, md5);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Queue saving: {e.Message}");
            }
        }

        public void SaveBackup()
        {
            try
            {
                var list = Queue.Select(p => JsonConvert.SerializeObject(new MonitoringPortOnDisk(p), JsonSerializerSettings)).ToList();
                File.WriteAllLines(_monitoringSettingFileBackup, list);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Queue saving: {e.Message}");
            }
        }

        public void ComposeNewQueue(List<PortWithTraceDto> ports)
        {
            var oldQueue = Queue.Select(p => p).ToList();
            Queue.Clear();

            foreach (var portWithTrace in ports)
            {
                var monitoringPort = new MonitoringPort(portWithTrace);
                var oldPort = oldQueue.FirstOrDefault(p => p.TraceId == monitoringPort.TraceId);
                if (oldPort != null)
                {
                    monitoringPort.LastMoniResult.UserReturnCode = oldPort.LastMoniResult.UserReturnCode;
                    monitoringPort.LastMoniResult.HardwareReturnCode = oldPort.LastMoniResult.HardwareReturnCode;
                    monitoringPort.LastTraceState = oldPort.LastTraceState;
                    monitoringPort.LastFastSavedTimestamp = oldPort.LastFastSavedTimestamp;
                    monitoringPort.LastPreciseMadeTimestamp = oldPort.LastPreciseMadeTimestamp;
                    monitoringPort.LastPreciseSavedTimestamp = oldPort.LastPreciseSavedTimestamp;
                }
                Queue.Enqueue(monitoringPort);
            }
        }

        public void RaiseMonitoringModeChangedFlag()
        {
            var temp = Queue.ToList();
            Queue.Clear();
            foreach (var monitoringPort in temp)
            {
                monitoringPort.IsMonitoringModeChanged = true;
                Queue.Enqueue(monitoringPort);
            }
        }

        static string CalculateMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

    }
}
