using System;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.Graph
{
    public class Rtu
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }

        public NetAddress MainChannel { get; set; } = new NetAddress(@"192.168.96.52", 11832);
        public RtuPartState MainChannelState { get; set; }
        public NetAddress ReserveChannel { get; set; } = new NetAddress("", 11832);
        public RtuPartState ReserveChannelState { get; set; }
        public bool IsReserveChannelSet { get; set; } = false;
        public NetAddress OtdrNetAddress { get; set; } = new NetAddress(@"192.168.96.52", 1500);
        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }

        public MonitoringState MonitoringState { get; set; }

    }
}
