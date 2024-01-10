using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class EventStatusFilter
    {
        public bool IsOn { get; set; }
        public EventStatus EventStatus { get; set; }

        public EventStatusFilter() { IsOn = false; }

        public EventStatusFilter(EventStatus eventStatus)
        {
            IsOn = true;
            EventStatus = eventStatus;
        }

        public override string ToString()
        {
            return IsOn ? EventStatus.GetLocalizedString() : Resources.SID__no_filter_;
        }
    }
}