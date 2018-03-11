using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class Node
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public EquipmentType TypeOfLastAddedEquipment { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Comment { get; set; }
    }
}