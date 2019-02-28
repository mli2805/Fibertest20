using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class Rtu
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }

        public NetAddress MainChannel { get; set; } = new NetAddress(@"192.168.96.0", TcpPorts.RtuListenTo);
        public RtuPartState MainChannelState { get; set; }
        public NetAddress ReserveChannel { get; set; } = new NetAddress("", TcpPorts.RtuListenTo);
        public RtuPartState ReserveChannelState { get; set; }
        public bool IsReserveChannelSet { get; set; } = false;
        public NetAddress OtdrNetAddress { get; set; } = new NetAddress(@"0.0.0.0", 1500); // real address
        public bool IsAvailable => MainChannelState == RtuPartState.Ok ||
                                   ReserveChannelState == RtuPartState.Ok;
        public bool IsAllRight => MainChannelState == RtuPartState.Ok &&
                                  ReserveChannelState != RtuPartState.Broken;

        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public bool IsInitialized => OwnPortCount != 0;
        public int FullPortCount { get; set; }

        public string PortCount => OwnPortCount == FullPortCount ? $@"{FullPortCount}" : $@"{OwnPortCount} / {FullPortCount}";

        public string Version { get; set; }

        public Dictionary<int, OtauDto> Children { get; set; } = new Dictionary<int, OtauDto>();

        public MonitoringState MonitoringState { get; set; }
        public Frequency PreciseMeas { get; set; } = Frequency.EveryHour;
        public Frequency PreciseSave { get; set; } = Frequency.DoNot;
        public Frequency FastSave { get; set; } = Frequency.DoNot;

        public TreeOfAcceptableMeasParams AcceptableMeasParams { get; set; } = new TreeOfAcceptableMeasParams();

        public List<Guid> ZoneIds { get; set; } = new List<Guid>();
    }
}
