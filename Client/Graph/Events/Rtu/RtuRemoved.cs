using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class RtuRemoved
    {
        public Guid RtuId { get; set; }
        public Guid RtuNodeId { get; set; }

        public Dictionary<Guid, Guid> FibersFromCleanedTraces { get; set; }
    }
}
