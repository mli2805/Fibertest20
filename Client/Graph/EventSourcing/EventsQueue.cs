using System.Collections.Generic;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class EventsQueue
    {
        public List<object> EventsWaitingForCommit { get; } = new List<object>();
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;

        public EventsQueue(IMyLog logFile, Model writeModel)
        {
            _logFile = logFile;
            _writeModel = writeModel;
        }

        public string Add(object e)
        {
            var result = _writeModel.Apply(e);
            if (result == null)
                EventsWaitingForCommit.Add(e);
            else
            {
                _logFile.AppendLine(result);
            }
            return result;

        }
        public void Commit()
        {
            EventsWaitingForCommit.Clear();
        }
    }
}