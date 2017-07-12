using System;

namespace Iit.Fibertest.Graph
{
    public class UpdateTrace
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public TraceMode Mode { get; set; }
        public string Comment { get; set; }
    }
}