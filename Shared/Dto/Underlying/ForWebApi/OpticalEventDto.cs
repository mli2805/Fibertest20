using System;

namespace Iit.Fibertest.Dto
{
    public class OpticalEventDto
    {
        public int EventId { get; set; }
        public DateTime MeasurementTimestamp { get; set; }
        public DateTime EventRegistrationTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public Guid RtuId { get; set; }

        public Guid TraceId { get; set; }
        public string TraceTitle { get; set; }

        public BaseRefType BaseRefType { get; set; }
        public FiberState TraceState { get; set; }

        public EventStatus EventStatus { get; set; }
        public DateTime StatusChangedTimestamp { get; set; }
        public string StatusChangedByUser { get; set; }

        public string Comment { get; set; }
    }
}