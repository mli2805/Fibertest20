using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class VeexOtdr
    {
        public bool CanVscout { get; set; }
        public IList<object> EnabledOptions { get; set; }
        public string Id { get; set; }
        public string MainframeId { get; set; }
        public string OpticalModuleSerialNumber { get; set; }
        public SupportedMeasurementParameters SupportedMeasurementParameters { get; set; }
        public TcpProxy TcpProxy { get; set; }
    }

    public class SupportedMeasurementParameters
    {
        public Dictionary<string, LaserUnit> LaserUnits { get; set; }
    }

    public class LaserUnit
    {
        public string Connector { get; set; }
        public Dictionary<string, DistanceRange> DistanceRanges { get; set; }
        public double DynamicRange { get; set; }
    }

    public class DistanceRange
    {
        public string[] AveragingTimes { get; set; }
        public string[] FastAveragingTimes { get; set; }
        public string[] PulseDurations { get; set; }
        public string[] Resolutions { get; set; }
    }

    public class TcpProxy
    {
        public string Self { get; set; }
    }
}