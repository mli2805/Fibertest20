using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class AssignBaseRef
    {
        public Guid TraceId { get; set; }

        public List<BaseRef> BaseRefs { get; set; } = new List<BaseRef>();
    }
}
