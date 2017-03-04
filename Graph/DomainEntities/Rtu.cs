﻿using System;

namespace Iit.Fibertest.Graph
{
    public class Rtu
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }

        public NetAddress MainChannel { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public NetAddress ReserveChannel { get; set; }
        public RtuPartState ReserveChannelState { get; set; }
        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }

        public MonitoringState MonitoringState { get; set; }

    }
}
