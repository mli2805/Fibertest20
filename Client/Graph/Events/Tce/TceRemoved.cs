using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class TceRemoved
    {
        public Guid Id { get; set; }
        public List<Guid> ExcludedTraceIds { get; set; }
    }
}