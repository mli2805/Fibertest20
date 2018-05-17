using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Newtonsoft.Json;
using NEventStore;

namespace Iit.Fibertest.Client
{
    public class EventToLogLineParser
    {
        public LogLine Parse(RtuAtGpsLocationAdded e) { return new LogLine() { OperationName = @"RTU added", RtuTitle = e.Title }; }
        public LogLine Parse(BaseRefAssigned e) { return new LogLine() { OperationName = @"Base ref assigned", OperationParams = e.TraceId.First6() }; }
    }
    public class EventLogViewModel : Screen
    {
        private readonly ILocalDbManager _localDbManager;
        private readonly EventToLogLineParser _eventToLogLineParser;

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        public List<LogLine> Rows { get; set; } = new List<LogLine>();

        public EventLogViewModel(ILocalDbManager localDbManager, EventToLogLineParser eventToLogLineParser)
        {
            _localDbManager = localDbManager;
            _eventToLogLineParser = eventToLogLineParser;
        }

        public async Task Initialize()
        {
            var jsonsInCache = await _localDbManager.LoadEvents();
            var ordinal = 1;
            foreach (var json in jsonsInCache)
            {
                var msg = (EventMessage)JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                if ((string)msg.Headers[@"Username"] == @"system") continue;

                var line = ParseEventBody(msg.Body);
                if (line == null) continue;

                line.Ordinal = ordinal;
                line.Username = (string)msg.Headers[@"Username"];
                line.ClientIp = (string)msg.Headers[@"ClientIp"];
                line.Timestamp = (DateTime)msg.Headers[@"Timestamp"];
                Rows.Insert(0, line);
                ordinal++;
            }
        }

        private LogLine ParseEventBody(object body)
        {
            switch (body)
            {
                case RtuAtGpsLocationAdded evnt: return _eventToLogLineParser.Parse(evnt);
                case BaseRefAssigned evnt: return _eventToLogLineParser.Parse(evnt);
                default: return null;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User_operations_log;
        }

        public void Close() { TryClose(); }
    }
}
