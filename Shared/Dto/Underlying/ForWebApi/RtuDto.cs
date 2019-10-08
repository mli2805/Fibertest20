using System;

namespace Iit.Fibertest.Dto
{
    public class RtuDto
    {
        public Guid RtuId { get; set; }
        public RtuMaker RtuMaker { get; set; }
        public string Title { get; set; }

        public NetAddress MainChannel { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public NetAddress ReserveChannel { get; set; }
        public RtuPartState ReserveChannelState { get; set; }
        public bool IsReserveChannelSet { get; set; }
        public NetAddress OtdrNetAddress { get; set; }
        public RtuPartState BopState { get; set; }

        public MonitoringState MonitoringMode { get; set; }

        public string Version { get; set; }
        public string Version2 { get; set; }

    }
}
