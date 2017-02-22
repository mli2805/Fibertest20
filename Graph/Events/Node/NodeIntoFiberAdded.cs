using System;

namespace Iit.Fibertest.Graph.Events
{
    [Serializable]
    public class NodeIntoFiberAdded
    {
        public Guid Id { get; set; }
        public GpsLocation Position { get; set; }
        public Guid FiberId { get; set; }
        public Guid NewFiberId1 { get; set; }
        public Guid NewFiberId2 { get; set; }


    }
}
