using System.Collections.Generic;
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
        public bool IsConfirmedChecked { get; set; } = true;

        public List<EventStatus> GetSelected()
        {
            var result = new List<EventStatus>();
            if (IsConfirmedChecked) result.Add(EventStatus.Confirmed);
            if (IsNotConfirmedChecked) result.Add(EventStatus.NotConfirmed);
            if (IsPlannedChecked) result.Add(EventStatus.Planned);
            if (IsSuspendedChecked) result.Add(EventStatus.Suspended);
            if (IsNotImportantChecked) result.Add(EventStatus.NotImportant);
            if (IsUnprocessedChecked) result.Add(EventStatus.Unprocessed);
            return result;
        }
    }
}
