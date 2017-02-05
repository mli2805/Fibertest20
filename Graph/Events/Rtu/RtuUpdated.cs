using System;

namespace Iit.Fibertest.Graph.Events
{
    public class RtuUpdated
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }

        public string Title { get; set; }
        public string Comment { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
