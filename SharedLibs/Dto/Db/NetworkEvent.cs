using System;

namespace Iit.Fibertest.Dto
{
    public class NetworkEvent
    {
        public int Id { get; set; }
        public DateTime EventTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public ChannelStateChanges MainChannelState { get; set; }
        public ChannelStateChanges ReserveChannelState { get; set; }

        public string BopString { get; set; } // 
    }
}