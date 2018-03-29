using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class TraceCleaned
    {
        public Guid TraceId { get; set; }

        public List<Guid> NodeIds { get; set; }
        public List<Guid> FiberIds { get; set; }
    }
}