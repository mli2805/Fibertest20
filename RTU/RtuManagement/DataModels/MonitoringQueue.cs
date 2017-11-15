using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public class MonitoringQueue
    {
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


        // Queue is stored on disk as json serialized list of MonitoringPortOnDisk
        public void Load()
        {
            _logFile.EmptyLine();
            _logFile.AppendLine("Monitoring queue assembling...");
            Queue = new Queue<MonitorigPort>();

            try
            {
                var contents = File.ReadAllLines(_monitoringSettingsFile);
                var javaScriptSerializer = new JavaScriptSerializer();

                var list = contents.Select(s => javaScriptSerializer.Deserialize<MonitoringPortOnDisk>(s)).ToList();
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
                var javaScriptSerializer = new JavaScriptSerializer();
                var list = Queue.Select(p => javaScriptSerializer.Serialize(new MonitoringPortOnDisk(p)));

                File.WriteAllLines(_monitoringSettingsFile, list);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Queue saving: {e.Message}");
            }
        }

        public void ComposeNewQueue(List<PortWithTraceDto> ports)
        {
            Queue.Clear();
            foreach (var portWithTrace in ports)
                Queue.Enqueue(new MonitorigPort(portWithTrace));
        }
    }
}
