using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.WpfForms
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
                Minor = LevelsContent.MinorLevelViewModel.IsFailed ? Resources.SID_fail : Resources.SID_pass;
                if (LevelsContent.MinorLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Minor;
            }
            if (LevelsContent.IsMajorExists)
            {
                Major = LevelsContent.MajorLevelViewModel.IsFailed ? Resources.SID_fail : Resources.SID_pass;
                if (LevelsContent.MajorLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Major;
            }
            if (LevelsContent.IsCriticalExists)
            {
                Critical = LevelsContent.CriticalLevelViewModel.IsFailed ? Resources.SID_fail : Resources.SID_pass;
                if (LevelsContent.CriticalLevelViewModel.IsFailed)
                    TraceState = Resources.SID_Critical;
            }
            if (LevelsContent.IsUsersExists)
            {
                Users = LevelsContent.UsersLevelViewModel.IsFailed ? Resources.SID_fail : Resources.SID_pass;
                if (LevelsContent.UsersLevelViewModel.IsFailed)
                    TraceState = Resources.SID_User_s;
            }
        }
    }
}
