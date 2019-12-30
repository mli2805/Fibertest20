using System.Collections.Generic;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class CombinedThreshold
    {
        public double min { get; set; }
        public double max { get; set; }
        public double decrease { get; set; }
        public double increase { get; set; }
    }

    public class ThresholdGroup
    {
        public CombinedThreshold event_loss { get; set; }
        public CombinedThreshold event_reflectance { get; set; }
        public CombinedThreshold event_leading_loss_coefficient { get; set; }
    }

    public class ThresholdScoupedGroup
    {
        public ThresholdGroup scope { get; set; }
        public ThresholdGroup thresholds { get; set; }
    }

    public class ThresholdLevel
    {
        public string name { get; set; }
        public List<ThresholdScoupedGroup> groups { get; set; }
    }

    public class TestThresholdSet
    {
        public List<ThresholdLevel> levels { get; set; }
    }
}
