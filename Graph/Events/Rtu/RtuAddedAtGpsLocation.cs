using System;

namespace Iit.Fibertest.Graph.Events
{
    public class RtuAddedAtGpsLocation
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }
}
