using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class EventsQueue
    {
        public List<object> EventsWaitingForCommit { get; } = new List<object>();
        private readonly WriteModel _writeModel;
        private readonly EventsOnModelExecutor _eventsOnModelExecutor;

        public EventsQueue(WriteModel writeModel, EventsOnModelExecutor eventsOnModelExecutor)
        {
            _writeModel = writeModel;
            _eventsOnModelExecutor = eventsOnModelExecutor;
        }

        public string Add(object e)
        {
//            var result = _writeModel.Add(e);
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