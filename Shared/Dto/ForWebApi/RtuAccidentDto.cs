using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class RtuAccidentDto
    {
        public int Id;
        public bool IsMeasurementProblem; // Measurement vs Rtu
        public ReturnCode ReturnCode;

        public DateTime EventRegistrationTimestamp;
        public Guid RtuId;
        public string RtuTitle;
        public Guid TraceId;
        public string TraceTitle;
        public BaseRefType BaseRefType;

        public string Comment;
    }

    public class RtuAccidentsRequestedDto
    {
        public int FullCount;
        public List<RtuAccidentDto> AccidentPortion;
    }
}
