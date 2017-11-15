using System;

namespace Iit.Fibertest.Dto
{
    public class NetworkEvent
    {
        public int Id { get; set; }
        public DateTime EventTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public RtuPart Part { get; set; }
        public RtuPartState PartState { get; set; }
    }
}