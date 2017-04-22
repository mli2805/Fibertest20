using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.RtuWpfExample
{
    public class RftsEventsOneLevelEeltViewModel
    {
        public string AttenuationValue { get; set; }
        public string Threshold { get; set; }
        public string DeviationValue { get; set; }
        public string StateValue { get; set; }

        public bool IsFailed { get; set; }

        public RftsEventsOneLevelEeltViewModel(double value, ShortThreshold threshold, ShortDeviation deviation)
        {
            AttenuationValue = $@"{value : 0.000}";
            Threshold = threshold.ForTable();
            DeviationValue = $@"{deviation.Deviation / 1000.0 : 0.000}";
            IsFailed = (deviation.Type & ShortDeviationTypes.IsExceeded) != 0;
            StateValue = IsFailed ? Resources.SID_fail : Resources.SID_pass;
        }
    }
}
