using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class Trace
    {
        public Guid Id { get; set; }

        public List<Node> Nodes { get; } = new List<Node>();
    }
}
