using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EventStatusComboItem
    {
        public EventStatus EventStatus { get; set; }

        public override string ToString()
        {
            return EventStatus.GetLocalizedString();
        }
    }
}