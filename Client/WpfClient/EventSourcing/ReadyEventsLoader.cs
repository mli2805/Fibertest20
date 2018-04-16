using System;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;
using NEventStore;

namespace Iit.Fibertest.Client
{
    public class ReadyEventsLoader
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly EventsOnTreeExecutor _eventsOnTreeExecutor;
        private readonly EventsOnModelExecutor _eventsOnModelExecutor;
        private readonly OpticalEventsExecutor _opticalEventsExecutor;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly RenderingManager _renderingManager;

        public ReadyEventsLoader(IMyLog logFile, ILocalDbManager localDbManager, IWcfServiceForClient c2DWcfManager, 
             EventsOnTreeExecutor eventsOnTreeExecutor,
            EventsOnModelExecutor eventsOnModelExecutor,
            OpticalEventsExecutor opticalEventsExecutor, 
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,  
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            RenderingManager renderingManager)
        {
            _logFile = logFile;
            _localDbManager = localDbManager;
            _c2DWcfManager = c2DWcfManager;
            _eventsOnTreeExecutor = eventsOnTreeExecutor;
            _eventsOnModelExecutor = eventsOnModelExecutor;
            _opticalEventsExecutor = opticalEventsExecutor;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _renderingManager = renderingManager;
        }

        public async Task<int> Load()
        {
            var currentEventNumber = await LoadFromCache();
            currentEventNumber = await LoadFromDb(currentEventNumber);
            // some sort of parsing snapshot
            _renderingManager.RenderGraphOnApplicationStart();
            _logFile.AppendLine(@"Rendering finished");
            return currentEventNumber;
        }

        private async Task<int> LoadFromCache()
        {
            var jsonsInCache = await _localDbManager.LoadEvents();
            _logFile.AppendLine($@"There are {jsonsInCache.Length} events in cache. Applying...");
            return ApplyBatch(jsonsInCache);
        }

        private async Task<int> LoadFromDb(int currentEventNumber)
        {
            string[] events;
            do
            {
                events = await _c2DWcfManager.GetEvents(currentEventNumber);
                await _localDbManager.SaveEvents(events);
                currentEventNumber = currentEventNumber + ApplyBatch(events);
            }
            while (events.Length != 0);
            _logFile.AppendLine($@"{currentEventNumber} events found in Cache + Db");
            return currentEventNumber;
        }

        private int ApplyBatch(string[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                var json = events[i];
                var msg = (EventMessage) JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                var evnt = msg.Body;

                try
                {
                    _eventsOnModelExecutor.Apply(evnt);
                    _eventsOnTreeExecutor.Apply(evnt);
                    _opticalEventsExecutor.Apply(evnt);
                    _networkEventsDoubleViewModel.Apply(evnt);
                    _bopNetworkEventsDoubleViewModel.Apply(evnt);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                    var header = @"Timestamp";
                    _logFile.AppendLine(
                        $@"Exception thrown while processing event with timestamp {msg.Headers[header]}");
                }
            }

            return events.Length;
        }
      
    }
}