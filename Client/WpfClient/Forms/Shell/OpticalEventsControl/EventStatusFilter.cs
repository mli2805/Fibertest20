using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class EventStatusFilter
    {
        public bool IsOn { get; set; }
        public EventStatus EventStatus { get; set; }

        /// <summary>
        /// таким конструктором создаетс€ ¬џключенный фильтр
        /// </summary>
        public EventStatusFilter() { IsOn = false; }

        /// <summary>
        /// а такой фильтр пропускает только "свое" значение
        /// </summary>
        /// <param name="eventStatus"></param>
        public EventStatusFilter(EventStatus eventStatus)
        {
            IsOn = true;
            EventStatus = eventStatus;
        }

        public override string ToString()
        {
            return IsOn ? EventStatus.GetLocalizedString() : "<no filter>";
        }
    }
}