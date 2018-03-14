using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class NetworkEventAdded
    {
        public Guid NetworkEventId { get; set; }

        public DateTime EventTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public RtuPartState ReserveChannelState { get; set; }
        public RtuPartStateChanges RtuPartStateChanges { get; set; }
    }
}