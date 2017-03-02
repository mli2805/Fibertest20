using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class RtuInitialized
    {
        public Guid Id { get; set; }
        public NetAddress MainChannel { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public NetAddress ReserveChannel { get; set; }
        public RtuPartState ReserveChannelState { get; set; }
        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
    }
}