using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class RtuAtGpsLocationAdded
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string Title { get; set; }
        public string Comment { get; set; }
    }
}
