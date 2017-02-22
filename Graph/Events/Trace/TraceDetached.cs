using System;

namespace Iit.Fibertest.Graph.Events
{
    [Serializable]
    public class TraceDetached
    {
        public Guid TraceId { get; set; }
    }
}
