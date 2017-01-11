using System;

namespace Iit.Fibertest.Graph.Commands
{
    public class AddEquipmentAtGpsLocation
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public EquipmentType Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }
}
