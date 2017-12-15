﻿using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class RtuInitialized
    {
        public Guid Id { get; set; }
        public NetAddress MainChannel { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public bool IsReserveChannelSet { get; set; } = false;
        public NetAddress ReserveChannel { get; set; }
        public RtuPartState ReserveChannelState { get; set; }
        public NetAddress OtauNetAddress { get; set; }
        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
        public List<OtauAttached> Otaus { get; set; } = new List<OtauAttached>();
        public string Version { get; set; }
        public bool IsMonitoringOn { get; set; }
    }
}