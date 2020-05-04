using System;
using GMap.NET;

namespace Iit.Fibertest.Graph
{
    public class AccidentLineModel
    {
        public string Caption { get; set; }
        public string TopLeft { get; set; }
        public string TopCenter { get; set; }
        public string TopRight { get; set; }
        public string Bottom0 { get; set; }
        public string Bottom1 { get; set; }
        public string Bottom2 { get; set; }
        public string Bottom3 { get; set; }
        public string Bottom4 { get; set; }

        public string PngPath { get; set; }
        public Uri Scheme => new Uri(PngPath);

        public PointLatLng? Position { get; set; }
    }
}