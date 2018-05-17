using System;

namespace Iit.Fibertest.Client
{
    public class LogLine
    {
        public int Ordinal { get; set; }
        public string Username { get; set; }
        public string ClientIp { get; set; }
        public DateTime Timestamp { get; set; }
        public string OperationName { get; set; }
        public string RtuTitle { get; set; }
        public string TraceTitle { get; set; }
        public string OperationParams { get; set; }
    }
}