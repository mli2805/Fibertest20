using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph.Commands
{
    public class RemoveNode
    {
        public Guid Id { get; set; }
        public Dictionary<Guid,Guid> TraceFiberPairForDetour { get; set; }
    }
}