using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class TraceAttached
    {
        public int Port { get; set; }
        public Guid TraceId { get; set; }
    }
}
