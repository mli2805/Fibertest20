using System;

namespace Iit.Fibertest.Client
{
    public class RequestAddEquipmentIntoNode
    {
        public Guid NodeId { get; set; }
        public bool IsCableReserveRequested { get; set; }
    }
}