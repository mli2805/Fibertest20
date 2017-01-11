using System;

namespace Iit.Fibertest.Graph.Events
{
    public class BaseRefAssigned
    {
        public Guid Id { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType Type { get; set; }
        public byte[] Content { get; set; }
    }
}
