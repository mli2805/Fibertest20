using System;

namespace Iit.Fibertest.Graph.Commands
{
    public class AssignBaseRef
    {
        public Guid TraceId { get; set; }

        public Guid PreciseId { get; set; }
        public Guid FastId { get; set; }
        public Guid AdditionalId { get; set; }

        public byte[] PreciseContent { get; set; }
        public byte[] FastContent { get; set; }
        public byte[] AdditionalContent { get; set; }
    }
}
