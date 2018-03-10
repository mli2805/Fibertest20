using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class EquipmentAtGpsLocationWithNodeTitleAdded
    {
        public Guid EmptyNodeEquipmentId { get; set; }
        public Guid RequestedEquipmentId { get; set; }
        public Guid NodeId { get; set; }
        public EquipmentType Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string Title { get; set; }
        public string Comment { get; set; }
    }
}