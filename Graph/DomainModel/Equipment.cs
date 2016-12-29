using System;

namespace Iit.Fibertest.Graph
{
    public enum EquipmentType
    {
        None = 0,
        CableReserve,
        Sleeve,
        Cross,
        Rtu,
        Other,
        Terminal,
    }
    public class Equipment
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public EquipmentType Type { get; set; } 
        public int CableReserveLeft { get; set; }
        public int CableReserveRight { get; set; }
        public string Comment { get; set; }

    }
}
