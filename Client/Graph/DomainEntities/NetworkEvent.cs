using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class NetworkEvent
    {
        public Guid NetworkEventId { get; set; }

        public DateTime EventTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public RtuPartState ReserveChannelState { get; set; }

        public bool IsAllRight => MainChannelState == RtuPartState.Ok && ReserveChannelState != RtuPartState.Broken;
    }
}