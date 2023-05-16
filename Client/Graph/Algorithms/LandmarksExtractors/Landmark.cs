using System;
using GMap.NET;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class Landmark
    {
        public bool IsFromBase { get; set; }
        public int Number { get; set; }
        public int NumberIncludingAdjustmentPoints { get; set; }
        public Guid NodeId { get; set; }
        public Guid FiberId { get; set; } // to the left
        public string NodeTitle { get; set; }
        public string NodeComment { get; set; }
        public Guid EquipmentId { get; set; }
        public string EquipmentTitle { get; set; }
        public EquipmentType EquipmentType { get; set; }
        public int LeftCableReserve { get; set; }
        public int RightCableReserve { get; set; }
        public double GpsDistance { get; set; }
        public double GpsSection { get; set; }
        public bool IsUserInput { get; set; }
        public double OpticalDistance { get; set; }
        public double OpticalSection { get; set; }
        public int EventNumber { get; set; }
        public PointLatLng GpsCoors { get; set; }

        public double UserInputLength
        {
            get => IsUserInput ? (GpsSection * 1000) : 0;
            set
            {
                if (value == 0)
                {
                    IsUserInput = false;
                    GpsSection = -1; // will be recalculated from nodes' coordinates
                }
                else
                {
                    IsUserInput = true;
                    GpsSection = value / 1000;
                }
            }
        }

        public bool AreCoordinatesTheSame(Landmark other)
        {
            return GpsCoors.ToDetailedString(GpsInputMode.DegreesMinutesAndSeconds) ==
                   other.GpsCoors.ToDetailedString(GpsInputMode.DegreesMinutesAndSeconds);
        }
    }
}