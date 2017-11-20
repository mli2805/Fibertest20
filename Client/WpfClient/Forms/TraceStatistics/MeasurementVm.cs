using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class MeasurementVm
    {
        public int Nomer { get; set; }
        public Guid MeasurementId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public DateTime Timestamp { get; set; }
        public FiberState TraceState { get; set; }
    }
}
