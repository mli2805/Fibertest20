using System;

namespace Iit.Fibertest.Graph.Events
{
    [Serializable]
    public class NodeUpdated
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Comment { get; set; }

    }
}