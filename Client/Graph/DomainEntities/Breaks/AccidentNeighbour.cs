using System;
using GMap.NET;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class AccidentNeighbour
    {
        public int LandmarkIndex { get; set; }
        public string Title { get; set; }
        public PointLatLng Coors { get; set; }
        public double ToRtuOpticalDistanceKm { get; set; }
        public double ToRtuPhysicalDistanceKm { get; set; }
    }
}