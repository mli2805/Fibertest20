using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iit.Fibertest.Graph.Events
{
    public class TraceAttached
    {
        public int Port { get; set; }
        public Guid TraceId { get; set; }
    }
}
