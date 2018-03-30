using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class UpdateEquipment
    {
        public Guid EquipmentId { get; set; }
        public string Title { get; set; }
        public EquipmentType Type { get; set; }
        public int CableReserveLeft { get; set; }
        public int CableReserveRight { get; set; }
        public string Comment { get; set; }
    }
}