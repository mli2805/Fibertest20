using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.RtuWpfExample
{
    public class RftsEventsFooterViewModel
    {
        public string State { get; set; }
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

            if (LevelsContent.IsMinorExists)
                Minor = LevelsContent.MinorLevelViewModel.IsFailed ? "fail" : "pass";
            if (LevelsContent.IsMajorExists)
                Major = LevelsContent.MajorLevelViewModel.IsFailed ? "fail" : "pass";
            if (LevelsContent.IsCriticalExists)
                Critical = LevelsContent.CriticalLevelViewModel.IsFailed ? "fail" : "pass";
            if (LevelsContent.IsUsersExists)
                Users = LevelsContent.UsersLevelViewModel.IsFailed ? "fail" : "pass";
        }
    }
}
