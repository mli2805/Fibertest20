using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class RtuRemoved
    {
        public Guid RtuId { get; set; }
        public Guid RtuNodeId { get; set; }

        // fiberId - traceId 
        // don't use Dictionary  because fiber could conduct more than one trace
        public List<KeyValuePair<Guid, Guid>> FibersFromCleanedTraces { get; set; }
    }
}
