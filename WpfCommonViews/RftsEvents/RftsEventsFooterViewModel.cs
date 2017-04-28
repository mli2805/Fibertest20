using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.WpfCommonViews
{
    public class RftsEventsFooterViewModel
    {
        public string TraceState { get; set; }
        public double Orl { get; set; }

        public string Minor { get; set; }
        public string Major { get; set; }
        public string Critical { get; set; }
        public string Users { get; set; }

        public LevelsContent LevelsContent { get; set; }

        public RftsEventsFooterViewModel(OtdrDataKnownBlocks sorData, LevelsContent levelsContent)
        {
            LevelsContent = levelsContent;
            Orl = sorData.KeyEvents.OpticalReturnLoss;

            SetStates(sorData);
        }

        private void SetStates(OtdrDataKnownBlocks sorData)
        {
            TraceState = Resources.SID_pass;
            if (LevelsContent.IsMinorExists)
            {
                Minor = LevelsContent.MinorLevelViewModel.IsFailed 
                    ? string.Format(Resources.SID_fail___0__km_, LevelsContent.MinorLevelViewModel.OneLevelTableContent.FirstProblemLocation) 
                    : Resources.SID_pass;
                if (LevelsContent.MinorLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Minor;
            }
            if (LevelsContent.IsMajorExists)
            {
                Major = LevelsContent.MajorLevelViewModel.IsFailed
                    ? string.Format(Resources.SID_fail___0__km_, LevelsContent.MajorLevelViewModel.OneLevelTableContent.FirstProblemLocation)
                    : Resources.SID_pass;
                if (LevelsContent.MajorLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Major;
            }
            if (LevelsContent.IsCriticalExists)
            {
                Critical = LevelsContent.CriticalLevelViewModel.IsFailed
                    ? string.Format(Resources.SID_fail___0__km_, LevelsContent.CriticalLevelViewModel.OneLevelTableContent.FirstProblemLocation)
                    : Resources.SID_pass;
                if (LevelsContent.CriticalLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Critical;
            }
            if (LevelsContent.IsUsersExists)
            {
                Users = LevelsContent.UsersLevelViewModel.IsFailed
                    ? string.Format(Resources.SID_fail___0__km_, LevelsContent.UsersLevelViewModel.OneLevelTableContent.FirstProblemLocation)
                    : Resources.SID_pass;
                if (LevelsContent.UsersLevelViewModel.IsFailed)
                    TraceState = Resources.SID_User_s;
            }
            if (sorData.RftsEvents.MonitoringResult == (int)ComparisonReturns.FiberBreak)
            {
                var owt = sorData.KeyEvents.KeyEvents[sorData.KeyEvents.KeyEventsCount - 1].EventPropagationTime;
                var breakLocation = sorData.OwtToLenKm(owt);
                TraceState = string.Format(Resources.SID_fiber_break___0_0_00000__km, breakLocation);
            }
        }
    }
}
