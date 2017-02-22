using System;

namespace Iit.Fibertest.Graph.Events
{
    [Serializable]
    public class NodeMoved
    {
        public Guid Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
