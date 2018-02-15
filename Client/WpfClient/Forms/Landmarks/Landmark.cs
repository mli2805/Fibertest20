using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class Landmark
    {
        public int Number { get; set; }
        public string NodeTitle { get; set; }
        public string EquipmentTitle { get; set; }
        public EquipmentType EquipmentType { get; set; }
        public double Location { get; set; }
        public int EventNumber { get; set; }
        public PointLatLng GpsCoors { get; set; }

        public LandmarkRow ToRow(GpsInputMode mode)
        {
            return new LandmarkRow()
            {
                Number = Number,
                NodeTitle = NodeTitle,
                EquipmentTitle = EquipmentTitle,
                EquipmentType = EquipmentType.ToLocalizedString(),
                Location = $@"{Location : 0.000}",
                EventNumber = EventNumber == 0 ? Resources.SID_no : $@"{EventNumber}",
                GpsCoors = GpsCoors.ToDetailedString(mode)
            };
        }
    }
}