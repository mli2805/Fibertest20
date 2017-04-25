namespace Iit.Fibertest.WpfForms
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
}