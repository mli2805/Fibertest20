using System.Collections.Generic;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class ThresholdSet
    {
        public List<Level> levels { get; set; }
    }

    public class Level
    {
        public List<Group> groups { get; set; }
        public string name { get; set; }
    }

    public class Group
    {
        public Thresholds thresholds { get; set; }
    }

    public class Thresholds
    {
        public CombinedThreshold eventLeadingLossCoefficient { get; set; }
        public CombinedThreshold eventLoss { get; set; }
        public CombinedThreshold eventMaxLevel { get; set; }  // PON
        public CombinedThreshold eventReflectance { get; set; }
    }

    public class CombinedThreshold
    {
        public double? min { get; set; }
        public double? max { get; set; }
        public double? decrease { get; set; }
        public double? increase { get; set; }
    }
}
