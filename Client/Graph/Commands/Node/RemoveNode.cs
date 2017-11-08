using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class RemoveNode
    {
        public Guid Id { get; set; }
        public Dictionary<Guid,Guid> TraceWithNewFiberForDetourRemovedNode { get; set; }
    }
}