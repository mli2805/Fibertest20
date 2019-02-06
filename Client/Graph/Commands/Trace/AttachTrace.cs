using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class AttachTrace
    {
        public Guid TraceId { get; set; }

        public OtauPortDto OtauPortDto { get; set; }

        public FiberState PreviousTraceState { get; set; }
        public List<AccidentOnTraceV2> AccidentsInLastMeasurement { get; set; }
    }
}
