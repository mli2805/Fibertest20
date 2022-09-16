using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace Iit.Fibertest.Dto
{
    public class VeexMeasOtdrParameters
    {
        public string measurementType { get; set; }
        public OpticalLineProperties opticalLineProperties { get; set; }
        public List<Laser> lasers { get; set; }
        public string distanceRange { get; set; }
        public string pulseDuration { get; set; }
        public string resolution { get; set; }
        public bool? fastMeasurement { get; set; } = null;
        public string averagingTime { get; set; }
        public bool? highFrequencyResolution { get; set; } = null;
        public List<RequiredConnectionQualitiesItem> requiredConnectionQualities { get; set; }
    }

    public class RequiredConnectionQualitiesItem
    {
        public double loss { get; set; }
        public double reflectance { get; set; }
    }
}