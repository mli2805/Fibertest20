using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class FiberAdded
    {
        public Guid FiberId { get; set; }
        public Guid NodeId1 { get; set; }
        public Guid NodeId2 { get; set; }
    }
}
