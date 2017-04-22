using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace RtuWpfExample
{
    public class RftsEventsOneLevelEeltViewModel
    {
        public double AttenuationValue { get; set; }
        public string Threshold { get; set; }
        public double DeviationValue { get; set; }
        public string StateValue { get; set; }

        public bool IsFailed { get; set; }

        public RftsEventsOneLevelEeltViewModel(double value, ShortThreshold threshold, ShortDeviation deviation)
        {
            AttenuationValue = value;
            Threshold = threshold.ForTable();
            DeviationValue = deviation.Deviation / 1000.0;
            IsFailed = (deviation.Type & ShortDeviationTypes.IsExceeded) != 0;
            StateValue = IsFailed ? "fail" : "pass";
        }
    }
}
