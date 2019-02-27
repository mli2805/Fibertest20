using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceStateSelectionViewModel : PropertyChangedBase
    {
        public string SuspicionContext => FiberState.Suspicion.ToLocalizedString();
        public string MinorContext => FiberState.Minor.ToLocalizedString();
        public string MajorContext => FiberState.Major.ToLocalizedString();
        public string CriticalContext => FiberState.Critical.ToLocalizedString();
        public string UserContext => FiberState.User.ToLocalizedString();
        public string FiberBreakContext => FiberState.FiberBreak.ToLocalizedString();
        public string NoFiberContext => FiberState.NoFiber.ToLocalizedString();

        public bool IsSuspicionChecked { get; set; }
        public bool IsMinorChecked { get; set; }
        public bool IsMajorChecked { get; set; }
        public bool IsCriticalChecked { get; set; }
        public bool IsUserChecked { get; set; }
        public bool IsFiberBreakChecked { get; set; }
        public bool IsNoFiberChecked { get; set; }
    }
}
