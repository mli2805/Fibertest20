﻿using System.IO;
using Caliburn.Micro;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.WpfCommonViews
{
    public class RftsEventsViewModel : Screen
    {
        public LevelsContent LevelsContent { get; set; } = new LevelsContent();
        public RftsEventsFooterViewModel FooterViewModel { get; set; }

        public int SelectedIndex { get; set; } = -1;

        public RftsEventsViewModel(OtdrDataKnownBlocks sorData)
        {
            var rftsParameters = sorData.RftsParameters;
            for (int i = 0; i < rftsParameters.LevelsCount; i++)
            {
                var level = rftsParameters.Levels[i];
                if (level.IsEnabled)
                    switch (level.LevelName)
                    {
                        case RftsLevelType.Minor:
                            LevelsContent.IsMinorExists = true;
                            LevelsContent.MinorLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                            if (SelectedIndex == -1) SelectedIndex = 0;
                            break;
                        case RftsLevelType.Major:
                            LevelsContent.IsMajorExists = true;
                            LevelsContent.MajorLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                            if (SelectedIndex == -1) SelectedIndex = 1;
                            break;
                        case RftsLevelType.Critical:
                            LevelsContent.IsCriticalExists = true;
                            LevelsContent.CriticalLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                            if (SelectedIndex == -1) SelectedIndex = 2;
                            break;
                        case RftsLevelType.None:
                            LevelsContent.IsUsersExists = true;
                            LevelsContent.UsersLevelViewModel = new RftsEventsOneLevelViewModel(sorData, level);
                            if (SelectedIndex == -1) SelectedIndex = 3;
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
