using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.RtuWpfExample
{
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
            DisplayName = Resources.SID_Events;
        }
    }
}
