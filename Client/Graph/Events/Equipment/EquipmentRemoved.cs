using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class EquipmentRemoved
    {
        public Guid EquipmentId { get; set; }
        public Guid NodeId { get; set; }
    }
}
