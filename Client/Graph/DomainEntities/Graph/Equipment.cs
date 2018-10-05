using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class Equipment
    {
        public Guid EquipmentId { get; set; }
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public EquipmentType Type { get; set; } 
        public int CableReserveLeft { get; set; }
        public int CableReserveRight { get; set; }
        public string Comment { get; set; }

    }
}
