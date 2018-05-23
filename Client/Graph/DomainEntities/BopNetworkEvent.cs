using System;

namespace Iit.Fibertest.Graph
{
    public class BopNetworkEvent
    {
        public int Ordinal { get; set; }

        public DateTime EventTimestamp { get; set; }
        public string OtauIp { get; set; }
        public int TcpPort { get; set; }
        public Guid RtuId { get; set; }
        public bool IsOk { get; set; }
    }
}