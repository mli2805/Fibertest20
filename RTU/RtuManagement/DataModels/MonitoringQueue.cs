using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public Queue<MonitorigPort> Queue { get; set; }

        public MonitoringQueue(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public int Count() { return Queue.Count; }
        public MonitorigPort Dequeue() { return Queue.Dequeue(); }
        public void Enqueue(MonitorigPort item) { Queue.Enqueue(item); }


        public void Load()
        {
            _logFile.EmptyLine();
            _logFile.AppendLine("Monitoring queue assembling...");
            Queue = new Queue<MonitorigPort>();

            try
            {
                var contents = File.ReadAllLines(_monitoringSettingsFile);

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
                
                var list = Queue.Select(p => JsonConvert.SerializeObject(new MonitoringPortOnDisk(p), JsonSerializerSettings));

                File.WriteAllLines(_monitoringSettingsFile, list);
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
                MonitorigPort oldPort;
                Queue.Enqueue(TryGetMonitoringPort(oldQueue, portWithTrace, out oldPort)
                    ? oldPort
                    : new MonitorigPort(portWithTrace));
            }
        }

        private bool TryGetMonitoringPort(List<MonitorigPort> oldQueue, PortWithTraceDto portWithTrace, out MonitorigPort theSameOldPort)
        {
            theSameOldPort = null;
            foreach (var oldPort in oldQueue)
            {
                if (oldPort.NetAddress.Ip4Address == portWithTrace.OtauPort.OtauIp
                    && oldPort.NetAddress.Port == portWithTrace.OtauPort.OtauTcpPort
                    && oldPort.OpticalPort == portWithTrace.OtauPort.OpticalPort)
                {
                    theSameOldPort = oldPort;
                    return true;
                }
            }
            return false;
        }

        public void RaiseMonitoringModeChangedFlag()
        {
            var temp = Queue.ToList();
            Queue.Clear();
            foreach (var monitorigPort in temp)
            {
                monitorigPort.MonitoringModeChangedFlag = true;
                Queue.Enqueue(monitorigPort);
            }
        }
    }
}
