using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Dto
{
    public enum EventStatus
    {
        JustMeasurementNotAnEvent = -99, // only for trace statistics
        EventButNotAnAccident = -9,  // Ok or Suspicion (made by Fast)

        NotImportant = -3,
        Planned = -2,
        NotConfirmed = -1,
        Unprocessed = 0,
        Suspended = 1,
        Confirmed = 2,
    }

    public static class EventStatusExt
    {
        public static string GetLocalizedString(this EventStatus eventStatus)
        {
            switch (eventStatus)
            {
                case EventStatus.JustMeasurementNotAnEvent:
                case EventStatus.EventButNotAnAccident:
                    return "";
                case EventStatus.NotImportant:
                    return Resources.SID_Not_important;
                case EventStatus.Planned:
                    return Resources.SID_Planned;
                case EventStatus.NotConfirmed:
                    return Resources.SID_Not_confirmed;
                case EventStatus.Unprocessed:
                    return Resources.SID_Unprocessed;
                case EventStatus.Suspended:
                    return Resources.SID_Suspended;
                case EventStatus.Confirmed:
                    return Resources.SID_Confirmed;
                default:
                    return Resources.SID_Unprocessed;
            }
        }

        public static bool IsStatusAssignedByUser(this EventStatus eventStatus)
        {
            return eventStatus > EventStatus.EventButNotAnAccident && eventStatus != EventStatus.Unprocessed;
        }
    }
}