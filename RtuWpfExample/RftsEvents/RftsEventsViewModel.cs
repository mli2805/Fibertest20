using Caliburn.Micro;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace RtuWpfExample
{
    public class LevelsContent
    {
        public bool IsMinorExists { get; set; }
        public bool IsMajorExists { get; set; }
        public bool IsCriticalExists { get; set; }
        public bool IsUsersExists { get; set; }

        public RftsEventsOneLevelViewModel MinorLevelViewModel { get; set; }
        public RftsEventsOneLevelViewModel MajorLevelViewModel { get; set; }
        public RftsEventsOneLevelViewModel CriticalLevelViewModel { get; set; }
        public RftsEventsOneLevelViewModel UsersLevelViewModel { get; set; }

    }
    public class RftsEventsViewModel : Screen
    {
        public LevelsContent LevelsContent { get; set; } = new LevelsContent();
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
                        LevelsContent.IsMinorExists = true;
                        LevelsContent.MinorLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                        break;
                    case RftsLevelType.Major:
                        LevelsContent.IsMajorExists = true;
                        LevelsContent.MajorLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                        break;
                    case RftsLevelType.Critical:
                        LevelsContent.IsCriticalExists = true;
                        LevelsContent.CriticalLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                        break;
                    case RftsLevelType.None:
                        LevelsContent.IsUsersExists = true;
                        LevelsContent.UsersLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                        break;
                }
            }

            FooterViewModel = new RftsEventsFooterViewModel(sorData, LevelsContent);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Rfts Events";
        }
    }
}
