using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph.Events
{
    public class EquipmentAdded
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public EquipmentType Type { get; set; }
        public int CableReserveLeft { get; set; }
        public int CableReserveRight { get; set; }
        public string Comment { get; set; }

        public List<Guid> TracesForInsertion { get; set; } = new List<Guid>();
    }
}
