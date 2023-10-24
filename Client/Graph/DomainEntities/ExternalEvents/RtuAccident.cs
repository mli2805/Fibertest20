using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class RtuAccident
    {
        public int Id { get; set; }
        public bool IsMeasurementProblem { get; set; } // Measurement vs Rtu
        public ReturnCode ReturnCode { get; set; }

        public DateTime EventRegistrationTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }

        public int ClearedAccidentWithId { get; set; } 


        public string Comment { get; set; }

        public bool IsGoodAccident =>
            ReturnCode == ReturnCode.MeasurementEndedNormally 
            || ReturnCode == ReturnCode.MeasurementErrorCleared
            || ReturnCode == ReturnCode.MeasurementErrorClearedByInit
            || ReturnCode == ReturnCode.RtuManagerServiceWorking;
    }
}