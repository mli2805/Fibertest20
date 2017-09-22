using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.UtilsLib;

namespace RtuManagement
{
    public class MonitoringQueue
    {
        private readonly IMyLog _logFile;
        private readonly string _monitoringSettingsFile = Utils.FileNameForSure(@"..\ini\", @"monitoring.que", false);
        public Queue<ExtendedPort> Queue { get; set; }

        public MonitoringQueue(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public int Count() { return Queue.Count; }
        public ExtendedPort Dequeue() { return Queue.Dequeue(); }
        public void Enqueue(ExtendedPort item) { Queue.Enqueue(item); }


        // Queue is stored on disk as json serialized list of PortInQueueOnDisk
        public void Load()
        {
            _logFile.EmptyLine();
            _logFile.AppendLine("Monitoring queue assembling...");
            Queue = new Queue<ExtendedPort>();

            try
            {
                var contents = File.ReadAllLines(_monitoringSettingsFile);
                var javaScriptSerializer = new JavaScriptSerializer();

                var list = contents.Select(s => javaScriptSerializer.Deserialize<PortInQueueOnDisk>(s)).ToList();
                foreach (var port in list)
                {
                    Queue.Enqueue(new ExtendedPort(port.NetAddress, port.OpticalPort, port.LastTraceState));
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
                var list = Queue.Select(p => javaScriptSerializer.Serialize(new PortInQueueOnDisk(p)));

                File.WriteAllLines(_monitoringSettingsFile, list);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Queue saving: {e.Message}");
            }
        }

        public void MergeNewPortsIntQueue(List<OtauPortDto> ports)
        {
            var newQueue = new Queue<ExtendedPort>();

            foreach (var otauPortDto in ports)
            {
                var extendedPort = Queue.FirstOrDefault(p => p.IsTheSamePort(otauPortDto)) ?? new ExtendedPort(otauPortDto);
                newQueue.Enqueue(extendedPort);
            }

            Queue = newQueue;
        }
    }
}
