using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class RtuAccidentAdded
    {
        public int Id { get; set; }
        public bool IsMeasurementProblem { get; set; } // Measurement vs Rtu
        public ReturnCode ReturnCode { get; set; }

        public DateTime EventRegistrationTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }

        public string Comment { get; set; }
    }
}