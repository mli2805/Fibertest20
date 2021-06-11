using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class ThresholdSet
    {
        public List<Level> Levels { get; set; }
    }

    public class Level
    {
        public AdvancedThresholds AdvancedThresholds { get; set; }
        public List<Group> Groups { get; set; }
        public string Name { get; set; }
    }

    public class AdvancedThresholds
    {
        public double AttenuationCoefficientChangeForNewEvents { get; set; }
        public double EofAttenuationCoefficientChangeForFiberBreak { get; set; }
        public double EofLossChangeForFiberBreak { get; set; }
        public double MaxEofAttenuationCoefficientForFiberBreak { get; set; }
        public double NoiseLevelChangeForFiberElongation { get; set; }
    }

    public class Group
    {
        public Thresholds Thresholds { get; set; }
    }

    public class Thresholds
    {
        public CombinedThreshold EventLeadingLossCoefficient { get; set; }
        public CombinedThreshold EventLoss { get; set; }
        public CombinedThreshold EventMaxLevel { get; set; }  // PON
        public CombinedThreshold EventReflectance { get; set; }
        public CombinedThreshold NonReflectiveEventPosition { get; set; } // UI in Advanced
        public CombinedThreshold ReflectiveEventPosition { get; set; } // UI in Advanced
    }

    public class CombinedThreshold
    {
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? Decrease { get; set; }
        public double? Increase { get; set; }
    }
}
