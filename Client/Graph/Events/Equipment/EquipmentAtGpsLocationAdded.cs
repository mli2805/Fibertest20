using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class EquipmentAtGpsLocationAdded
    {
        public Guid EmptyNodeEquipmentId { get; set; }
        public Guid RequestedEquipmentId { get; set; }
        public Guid NodeId { get; set; }
        public EquipmentType Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
