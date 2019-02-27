using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class EventStatusViewModel : PropertyChangedBase
    {
        public string NotImportantContext => EventStatus.NotImportant.GetLocalizedString();
        public string PlannedContext => EventStatus.Planned.GetLocalizedString();
        public string NotConfirmedContext => EventStatus.NotConfirmed.GetLocalizedString();
        public string UnprocessedContext => EventStatus.Unprocessed.GetLocalizedString();
        public string SuspendedContext => EventStatus.Suspended.GetLocalizedString();
        public string ConfirmedContext => EventStatus.Confirmed.GetLocalizedString();

        public bool IsNotImportantChecked { get; set; }
        public bool IsPlannedChecked { get; set; }
        public bool IsNotConfirmedChecked { get; set; }
        public bool IsUnprocessedChecked { get; set; }
        public bool IsSuspendedChecked { get; set; }
        public bool IsConfirmedChecked { get; set; }


    }
}
