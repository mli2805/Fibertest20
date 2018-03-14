using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class BopNetworkEvent
    {
        public Guid BopNetworkEventId { get; set; }

        public DateTime EventTimestamp { get; set; }
        public Guid BopId { get; set; }
        public Guid RtuId { get; set; }
        public RtuPartState State { get; set; }
    }
}