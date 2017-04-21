using Caliburn.Micro;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace RtuWpfExample
{
    public class RftsEventsViewModel : Screen
    {
        public bool IsMinorExists { get; set; }
        public bool IsMajorExists { get; set; }
        public bool IsCriticalExists { get; set; }
        public bool IsUsersExists { get; set; }

        public RftsEventsOneLevelViewModel MinorLevelViewModel { get; set; }
        public RftsEventsOneLevelViewModel MajorLevelViewModel { get; set; }
        public RftsEventsOneLevelViewModel CriticalLevelViewModel { get; set; }
        public RftsEventsOneLevelViewModel UsersLevelViewModel { get; set; }

        public RftsEventsFooterViewModel FooterViewModel { get; set; }

        public RftsEventsViewModel(OtdrDataKnownBlocks sorData)
        {
            var rftsParameters = sorData.RftsParameters;
            for (int i = 0; i < rftsParameters.LevelsCount; i++)
            {
                var level = rftsParameters.Levels[i];
                switch (level.LevelName)
                {
                    case RftsLevelType.Minor:
                        IsMinorExists = true;
                        MinorLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                        break;
                    case RftsLevelType.Major:
                        IsMajorExists = true;
                        MajorLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                        break;
                    case RftsLevelType.Critical:
                        IsCriticalExists = true;
                        CriticalLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                        break;
                    case RftsLevelType.None:
                        IsUsersExists = true;
                        UsersLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                        break;
                }
            }

            FooterViewModel = new RftsEventsFooterViewModel(sorData);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Rfts Events";
        }
    }
}
