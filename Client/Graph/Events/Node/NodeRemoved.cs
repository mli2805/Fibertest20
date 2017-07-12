using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class NodeRemoved
    {
        public Guid Id { get; set; }
        public Dictionary<Guid,Guid> TraceFiberPairForDetour { get; set; }
    }
}