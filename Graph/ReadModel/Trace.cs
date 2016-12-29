using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iit.Fibertest.Graph
{
    public class Trace
    {
        public Guid Id { get; set; }

        public List<Node> Nodes { get; } = new List<Node>();
    }
}
