using System;
using GMap.NET;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class Landmark : ICloneable
    {
        public int Number { get; set; }
        public int NumberIncludingEmptyWells { get; set; }
        public Guid NodeId { get; set; }
        public string NodeTitle { get; set; }
        public string NodeComment { get; set; }
        public Guid EquipmentId { get; set; }
        public string EquipmentTitle { get; set; }
        public EquipmentType EquipmentType { get; set; }
        public double Distance { get; set; }
        public int EventNumber { get; set; }
        public PointLatLng GpsCoors { get; set; }
        
        public object Clone()
        {
            return new Landmark()
            {
                Number = Number,
                NumberIncludingEmptyWells = NumberIncludingEmptyWells,
                NodeId = NodeId,
                NodeTitle = NodeTitle,
                NodeComment = NodeComment,
                EquipmentId = EquipmentId,
                EquipmentTitle = EquipmentTitle,
                EquipmentType = EquipmentType,
                Distance = Distance,
                EventNumber = EventNumber,
                GpsCoors = new PointLatLng(GpsCoors.Lat, GpsCoors.Lng),
            };
        }
    }
}