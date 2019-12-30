using System.Collections.Generic;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class EventLeadingLossCoefficient
    {
        public double decrease { get; set; }
        public double increase { get; set; }
    }

    public class EventLoss
    {
        public double decrease { get; set; }
        public double increase { get; set; }
    }

    public class EventMaxLevel
    {
        public int decrease { get; set; }
        public int increase { get; set; }
    }

    public class EventReflectance
    {
        public int decrease { get; set; }
        public int increase { get; set; }
    }

    public class Thresholds
    {
        public EventLeadingLossCoefficient eventLeadingLossCoefficient { get; set; }
        public EventLoss eventLoss { get; set; }
        public EventMaxLevel eventMaxLevel { get; set; }
        public EventReflectance eventReflectance { get; set; }
    }

    public class Group
    {
        public Thresholds thresholds { get; set; }
    }

    public class Level
    {
        public List<Group> groups { get; set; }
        public string name { get; set; }
    }

    public class ThresholdSet
    {
        public List<Level> levels { get; set; }
    }
}
