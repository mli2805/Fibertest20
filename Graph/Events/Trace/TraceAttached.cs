using System;

namespace Iit.Fibertest.Graph.Events
{
    public class TraceAttached
    {
        public int Port { get; set; }
        public Guid TraceId { get; set; }
    }
}
