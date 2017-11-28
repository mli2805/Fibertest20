using Iit.Fibertest.Dto;

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