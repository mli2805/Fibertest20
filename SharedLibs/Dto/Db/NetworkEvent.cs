using System;

namespace Iit.Fibertest.Dto
{
    public class NetworkEvent
    {
        public int Id { get; set; }
        public DateTime EventTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public RtuPartState ReserveChannelState { get; set; }

        public bool IsAllRight => MainChannelState == RtuPartState.Ok && ReserveChannelState != RtuPartState.Broken;
    }
}