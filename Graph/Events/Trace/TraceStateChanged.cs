using System;

namespace Iit.Fibertest.Graph.Events
{
    [Serializable]
    public class TraceStateChanged
    {
        public Guid TraceId { get; set; }
        public FiberState State { get; set; }
    }
}