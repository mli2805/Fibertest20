using System;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class Landmark : ICloneable
    {
        public int Number { get; set; }
        public int NumberIncludingAdjustmentPoints { get; set; }
        public Guid NodeId { get; set; }
        public string NodeTitle { get; set; }
        public string NodeComment { get; set; }
        public Guid EquipmentId { get; set; }
        public string EquipmentTitle { get; set; }
        public EquipmentType EquipmentType { get; set; }
        public double Distance { get; set; }
        public int EventNumber { get; set; }
        public PointLatLng GpsCoors { get; set; }

        public LandmarkRow ToRow(GpsInputMode mode)
        {
            return new LandmarkRow()
            {
                Number = Number,
                NumberIncludingAdjustmentPoints = NumberIncludingAdjustmentPoints,
                NodeId = NodeId,
                NodeTitle = NodeTitle,
                NodeComment = NodeComment,
                EquipmentId = EquipmentId,
                EquipmentTitle = EquipmentTitle,
                EquipmentType = EquipmentType.ToLocalizedString(),
                Distance = $@"{Distance : 0.000}",
                EventNumber = EventNumber == -1 ? Resources.SID_no : $@"{EventNumber}",
                GpsCoors = GpsCoors.ToDetailedString(mode)
            };
        }

        public object Clone()
        {
            return new Landmark()
            {
                Number = Number,
                NumberIncludingAdjustmentPoints = NumberIncludingAdjustmentPoints,
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