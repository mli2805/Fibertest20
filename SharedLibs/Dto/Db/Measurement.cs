using System;

namespace Iit.Fibertest.Dto
{
    public class Measurement
    {
        public int Id { get; set; }
        public Guid MeasurementId { get; set; }
        public Guid RtuId { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public DateTime Timestamp { get; set; }
        public FiberState TraceState { get; set; }
    }
}