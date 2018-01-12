using System;

namespace Iit.Fibertest.Graph
{
    public class AddEquipmentAtGpsLocation
    {
        public Guid EmptyNodeEquipmentId { get; set; } 
        public Guid RequestedEquipmentId { get; set; } 
        public Guid NodeId { get; set; }
        public EquipmentType Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }
}
