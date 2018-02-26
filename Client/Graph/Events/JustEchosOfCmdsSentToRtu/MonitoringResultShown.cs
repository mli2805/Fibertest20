using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class MonitoringResultShown
    {
        public Guid TraceId { get; set; }
        public FiberState TraceState { get; set; }
        public List<AccidentOnTrace> Accidents { get; set; }
    }
}