using System;

namespace Iit.Fibertest.Graph.Requests
{
    public class RequestAddTrace
    {
        public Guid NodeWithRtuId { get; set; }
        public Guid LastNodeId { get; set; }
    }
}
