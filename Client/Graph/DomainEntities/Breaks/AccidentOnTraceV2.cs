using System;
using GMap.NET;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class AccidentOnTraceV2
    {
        public int BrokenRftsEventNumber { get; set; }

        public FiberState AccidentSeriousness { get; set; }
        public OpticalAccidentType OpticalTypeOfAccident { get; set; }
        
        public bool IsAccidentInOldEvent { get; set; }
        public bool IsAccidentInLastNode { get; set; }
        public PointLatLng AccidentCoors { get; set; }

        public int AccidentLandmarkIndex { get; set; }
        public double AccidentToRtuOpticalDistanceKm { get; set; }
        public string AccidentTitle { get; set; }
        public double AccidentToRtuPhysicalDistanceKm { get; set; }

        public double AccidentToLeftOpticalDistanceKm { get; set; }
        public double AccidentToLeftPhysicalDistanceKm { get; set; }
        public double AccidentToRightOpticalDistanceKm { get; set; }
        public double AccidentToRightPhysicalDistanceKm { get; set; }

        public string EventCode { get; set; }
        public double DeltaLen { get; set; }

        public AccidentNeighbour Left { get; set; }
        public AccidentNeighbour Right { get; set; }

        public bool IsTheSame(AccidentOnTraceV2 other)
        {
            if (BrokenRftsEventNumber != other.BrokenRftsEventNumber) return false;
            if (AccidentSeriousness != other.AccidentSeriousness) return false;
            if (OpticalTypeOfAccident != other.OpticalTypeOfAccident) return false;
            if (IsAccidentInOldEvent != other.IsAccidentInOldEvent) return false;

            return Math.Abs(AccidentToRtuOpticalDistanceKm - other.AccidentToRtuOpticalDistanceKm) < DeltaLen;
        }
    }
}