using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class InitializeRtu
    {
        public Guid Id { get; set; }
        public NetAddress MainChannel { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public NetAddress ReserveChannel { get; set; }
        public RtuPartState ReserveChannelState { get; set; }
        public NetAddress OtdrNetAddress { get; set; }
        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }

        public List<AttachOtau> Otaus { get; set; } = new List<AttachOtau>();
    }
}