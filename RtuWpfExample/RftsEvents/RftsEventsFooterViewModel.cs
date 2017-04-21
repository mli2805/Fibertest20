using Optixsoft.SorExaminer.OtdrDataFormat;

namespace RtuWpfExample
{
    public class RftsEventsFooterViewModel
    {
        public string State { get; set; }
        public double Orl { get; set; }

        public string Minor { get; set; }
        public string Major { get; set; }
        public string Critical { get; set; }
        public string Users { get; set; }

        public bool IsMinorExists { get; set; }
        public bool IsMajorExists { get; set; }
        public bool IsCriticalExists { get; set; }
        public bool IsUsersExists { get; set; }


        public RftsEventsFooterViewModel(OtdrDataKnownBlocks sorData)
        {
            Orl = sorData.KeyEvents.OpticalReturnLoss;
            var rftsParameters = sorData.RftsParameters;

            for (int i = 0; i < rftsParameters.LevelsCount; i++)
            {
                var level = rftsParameters.Levels[i];
                switch (level.LevelName)
                {
                    case RftsLevelType.Minor:
                        IsMinorExists = true;
                        break;
                    case RftsLevelType.Major:
                        IsMajorExists = true;
                        break;
                    case RftsLevelType.Critical:
                        IsCriticalExists = true;
                        break;
                    case RftsLevelType.None:
                        IsUsersExists = true;
                        break;
                }
            }
        }
    }
}
