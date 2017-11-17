using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class NetworkEventVm
    {
        public int Nomer { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public ChannelStateChanges MainChannelState { get; set; }
        public ChannelStateChanges ReserveChannelState { get; set; }

        public string BopString { get; set; } // 
    }
}
