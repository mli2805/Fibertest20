using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class InitializeRtu
    {
        public Guid Id { get; set; }
        public RtuMaker Maker { get; set; }

        public string OtauId { get; set; } // in VeEX RTU main OTAU has its own ID
        public string OtdrId { get; set; } // ditto

        public string Mfid { get; set; }
        public string Mfsn { get; set; }
        public string Omid { get; set; }
        public string Omsn { get; set; }

        public NetAddress MainChannel { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public bool IsReserveChannelSet { get; set; } = false;
        public NetAddress ReserveChannel { get; set; }
        public RtuPartState ReserveChannelState { get; set; }
        public NetAddress OtauNetAddress { get; set; } // IP the same as Otdr, Charon
        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
        public string Version { get; set; }
        public string Version2 { get; set; }

        public Dictionary<int, OtauDto> Children { get; set; }
        public bool IsMonitoringOn { get; set; }
        public TreeOfAcceptableMeasParams AcceptableMeasParams { get; set; } = new TreeOfAcceptableMeasParams();
    }
}