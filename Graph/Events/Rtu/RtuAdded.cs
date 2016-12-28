using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iit.Fibertest.Graph.Events
{
    public class RtuAdded
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
    }
}
