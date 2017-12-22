using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class NetworkEventModel
    {
        public int Nomer { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public RtuPartState ReserveChannelState { get; set; }

        public string BopString { get; set; } // 
    }
}
