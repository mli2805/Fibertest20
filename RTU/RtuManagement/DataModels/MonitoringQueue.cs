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
        public Queue<MonitorigPort> Queue { get; set; }

        public MonitoringQueue(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public int Count() { return Queue.Count; }
        public MonitorigPort Dequeue() { return Queue.Dequeue(); }
        public void Enqueue(MonitorigPort item) { Queue.Enqueue(item); }

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

        public void Load()
        {
            _logFile.EmptyLine();
            _logFile.AppendLine("Monitoring queue assembling...");
            Queue = new Queue<MonitorigPort>();

            try
            {

                var contents = LoadWithMd5();
                var list = contents.Select(s => (MonitoringPortOnDisk)JsonConvert.DeserializeObject(s, JsonSerializerSettings)).ToList();

                foreach (var port in list)
                {
                    Queue.Enqueue(new MonitorigPort(port));
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
                MonitorigPort theSamePortInOldQueue = TryGetTheSameMonitoringPortInOldQueue(oldQueue, portWithTrace);
                if (theSamePortInOldQueue != null)
                {
                    theSamePortInOldQueue.TraceId = portWithTrace.TraceId;
                    Queue.Enqueue(theSamePortInOldQueue);
                }
                else
                    Queue.Enqueue(new MonitorigPort(portWithTrace));
            }
        }

        private MonitorigPort TryGetTheSameMonitoringPortInOldQueue(List<MonitorigPort> oldQueue, PortWithTraceDto portWithTrace)
        {
            foreach (var portInOldQueue in oldQueue)
            {
                if (portInOldQueue.CharonSerial == portWithTrace.OtauPort.Serial
                     && portInOldQueue.OpticalPort == portWithTrace.OtauPort.OpticalPort)
                {
                    return portInOldQueue;
                }
            }
            return null;
        }

        public void RaiseMonitoringModeChangedFlag()
        {
            var temp = Queue.ToList();
            Queue.Clear();
            foreach (var monitorigPort in temp)
            {
                monitorigPort.IsMonitoringModeChanged = true;
                Queue.Enqueue(monitorigPort);
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
