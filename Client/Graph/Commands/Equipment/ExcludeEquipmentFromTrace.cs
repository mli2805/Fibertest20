using System;

namespace Iit.Fibertest.Graph
{
    public class ExcludeEquipmentFromTrace
    {
        public Guid EquipmentId { get; set; }
        public Guid TraceId { get; set; }
        public int IndexInTrace { get; set; }
    }
}