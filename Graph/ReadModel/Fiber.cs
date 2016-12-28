using System;

namespace Iit.Fibertest.Graph
{
    public class Fiber
    {
        public Guid Id { get; set; }
        public Guid Node1 { get; set; }
        public Guid Node2 { get; set; }

        public double GpsLength { get; set; }
        public bool ShouldGpsLengthBeChangedOnNodeMove { get; set; } = true;

        public double OpticalLength { get; set; }
    }
}