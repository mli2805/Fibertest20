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

            SetStates();
        }

        private void SetStates()
        {
            TraceState = Resources.SID_pass;
            if (LevelsContent.IsMinorExists)
            {
                Minor = LevelsContent.MinorLevelViewModel.IsFailed 
                    ? string.Format(Resources.SID_fail___0__km_, LevelsContent.MinorLevelViewModel.EventContent.FirstProblemLocation) 
                    : Resources.SID_pass;
                if (LevelsContent.MinorLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Minor;
            }
            if (LevelsContent.IsMajorExists)
            {
                Major = LevelsContent.MajorLevelViewModel.IsFailed
                    ? string.Format(Resources.SID_fail___0__km_, LevelsContent.MajorLevelViewModel.EventContent.FirstProblemLocation)
                    : Resources.SID_pass;
                if (LevelsContent.MajorLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Major;
            }
            if (LevelsContent.IsCriticalExists)
            {
                Critical = LevelsContent.CriticalLevelViewModel.IsFailed
                    ? string.Format(Resources.SID_fail___0__km_, LevelsContent.CriticalLevelViewModel.EventContent.FirstProblemLocation)
                    : Resources.SID_pass;
                if (LevelsContent.CriticalLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Critical;
            }
            if (LevelsContent.IsUsersExists)
            {
                Users = LevelsContent.UsersLevelViewModel.IsFailed
                    ? string.Format(Resources.SID_fail___0__km_, LevelsContent.UsersLevelViewModel.EventContent.FirstProblemLocation)
                    : Resources.SID_pass;
                if (LevelsContent.UsersLevelViewModel.IsFailed)
                    TraceState = Resources.SID_User_s;
            }
        }
    }
}
