using System;

namespace Iit.Fibertest.Dto
{
    public class BopNetworkEvent
    {
        public int Id { get; set; }
        public DateTime EventTimestamp { get; set; }
        public Guid BopId { get; set; }
        public Guid RtuId { get; set; }
        public RtuPartState State { get; set; }

    }
}