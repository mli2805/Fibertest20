using System;

namespace DbExperiments
{
    [Serializable]
    public class MonitoringResult
    {
        public int Id { get; set; }
        public Guid RtuId { get; set; }
        public Guid TraceId { get; set; }
        public DateTime Timestamp { get; set; }

        public byte[] SorBytes { get; set; }
    }
}