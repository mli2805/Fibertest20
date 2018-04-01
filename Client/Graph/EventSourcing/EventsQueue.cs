using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class EventsQueue
    {
        public List<object> EventsWaitingForCommit { get; } = new List<object>();
        private readonly EventsOnModelExecutor _eventsOnModelExecutor;

        public EventsQueue(EventsOnModelExecutor eventsOnModelExecutor)
        {
            _eventsOnModelExecutor = eventsOnModelExecutor;
        }

        public string Add(object e)
        {
            var result = _eventsOnModelExecutor.Apply(e);
            if (result == null)
                EventsWaitingForCommit.Add(e);
            return result;

        }
        public void Commit()
        {
            EventsWaitingForCommit.Clear();
        }
    }
}