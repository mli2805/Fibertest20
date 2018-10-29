using System;

namespace Iit.Fibertest.Graph
{
    public class EventReportModel
    {
        public string TraceTitle { get; set; }
        public string TraceState { get; set; }
        public string RtuTitle { get; set; }
        public string Port { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}