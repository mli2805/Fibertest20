using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace RtuWpfExample
{
    public class RftsEventsOneLevelEeltViewModel
    {
        public double AttenuationValue { get; set; }
        public string Threshold { get; set; }
        public double DeviationValue { get; set; }
        public string StateValue { get; set; }

        public RftsEventsOneLevelEeltViewModel(RftsLevel rftsLevel, double value)
        {
            AttenuationValue = value;
            Threshold = rftsLevel.EELT.ForTable();
        }
    }
}
