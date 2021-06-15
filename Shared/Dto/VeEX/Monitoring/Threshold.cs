using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.Dto
{
    public class ThresholdSet
    {
        public List<Level> levels { get; set; }
    }

    public class Level
    {
        public AdvancedThresholds advancedThresholds { get; set; }
        public List<Group> groups { get; set; }
        public string name { get; set; }
    }

    public class AdvancedThresholds
    {
        public double attenuationCoefficientChangeForNewEvents { get; set; }
        public double eofAttenuationCoefficientChangeForFiberBreak { get; set; }
        public double eofLossChangeForFiberBreak { get; set; }
        public double maxEofAttenuationCoefficientForFiberBreak { get; set; }
        public double noiseLevelChangeForFiberElongation { get; set; }
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
        public CombinedThreshold nonReflectiveEventPosition { get; set; } // UI in Advanced
        public CombinedThreshold reflectiveEventPosition { get; set; } // UI in Advanced
    }

    public class CombinedThreshold
    {
        public double? min { get; set; }
        public double? max { get; set; }
        public double? decrease { get; set; }
        public double? increase { get; set; }
    }
}
