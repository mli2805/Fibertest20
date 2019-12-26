using System.Collections.Generic;

namespace HttpLib
{
    public class Otdr
    {
        public bool canVscout { get; set; }
        public IList<object> enabledOptions { get; set; }
        public string id { get; set; }
        public string mainframeId { get; set; }
        public string opticalModuleSerialNumber { get; set; }
        public SupportedMeasurementParameters supportedMeasurementParameters { get; set; }
        public TcpProxy tcpProxy { get; set; }
    }

    public class SupportedMeasurementParameters
    {
        public Dictionary<string, LaserUnit> laserUnits { get; set; }
    }

    public class LaserUnit
    {
        public string connector { get; set; }
        public Dictionary<string, DistanceRange> distanceRanges { get; set; }
        public double dynamicRange { get; set; }
    }

    public class DistanceRange
    {
        public string[] averagingTimes { get; set; }
        public string[] fastAveragingTimes { get; set; }
        public string[] pulseDurations { get; set; }
        public string[] resolutions { get; set; }
    }

    public class TcpProxy
    {
        public string self { get; set; }
    }
}