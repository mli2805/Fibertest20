using System;

namespace Iit.Fibertest.Graph
{
    public class IncludeEquipmentIntoTrace
    {
        public Guid EquipmentId { get; set; }
        public Guid TraceId { get; set; }
        public int IndexInTrace { get; set; }
    }
}