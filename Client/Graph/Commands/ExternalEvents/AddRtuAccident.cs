using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class AddRtuAccident
    {
        public bool IsMeasurementProblem { get; set; } // Measurement vs Rtu
        public ReturnCode ReturnCode { get; set; }

        public DateTime EventRegistrationTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public int ClearedAccidentWithId { get; set; } 

        public string Comment { get; set; }
    }
}