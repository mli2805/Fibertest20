using System;

namespace Iit.Fibertest.Graph
{
    public class AddBopNetworkEvent
    {
        public DateTime EventTimestamp { get; set; }
        public string OtauIp { get; set; }
        public Guid RtuId { get; set; }
        public bool IsOk { get; set; }
    }
}