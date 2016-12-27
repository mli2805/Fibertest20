using System;

namespace Iit.Fibertest.Graph.Events
{
    public class NodeAdded
    {
        public Guid Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }
}