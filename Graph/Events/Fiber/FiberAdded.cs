using System;

namespace Iit.Fibertest.Graph.Events
{
    [Serializable]
    public class FiberAdded
    {
        public Guid Id { get; set; }
        public Guid Node1 { get; set; }
        public Guid Node2 { get; set; }
    }
}
