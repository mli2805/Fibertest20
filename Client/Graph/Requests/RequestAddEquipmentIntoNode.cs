using System;

namespace Iit.Fibertest.Graph.Requests
{
    public class RequestAddEquipmentIntoNode
    {
        public Guid NodeId { get; set; }
        public bool IsCableReserveRequested { get; set; }
    }
}