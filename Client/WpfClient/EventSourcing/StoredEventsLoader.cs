using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;
using NEventStore;

namespace Iit.Fibertest.Client
{
    public class StoredEventsLoader
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private Model _readModel;
        private readonly EventsOnTreeExecutor _eventsOnTreeExecutor;
        private readonly OpticalEventsExecutor _opticalEventsExecutor;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly RenderingManager _renderingManager;

        public StoredEventsLoader(IMyLog logFile, ILocalDbManager localDbManager,
            CurrentDatacenterParameters currentDatacenterParameters, IWcfServiceForClient c2DWcfManager, Model readModel,
             EventsOnTreeExecutor eventsOnTreeExecutor,
            OpticalEventsExecutor opticalEventsExecutor,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            RenderingManager renderingManager)
        {
            _logFile = logFile;
            _localDbManager = localDbManager;
            _currentDatacenterParameters = currentDatacenterParameters;
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
            _eventsOnTreeExecutor = eventsOnTreeExecutor;
            _opticalEventsExecutor = opticalEventsExecutor;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _renderingManager = renderingManager;
        }

        // TwoComponentLoading
        public async Task<int> TwoComponentLoading()
        {
            var lastEventFromSnapshot = await LoadAndDeserializeSnapshot();
            var lastLoadedEvent = lastEventFromSnapshot + await LoadAndApplyEvents(lastEventFromSnapshot);
            var currentEventNumber = await DownloadAndApplyEvents(lastLoadedEvent);

            _renderingManager.Initialize();
            await _renderingManager.RenderCurrentZoneOnApplicationStart();
            return currentEventNumber;
        }

        private async Task<int> LoadAndDeserializeSnapshot()
        {
            if (_currentDatacenterParameters.SnapshotLastEvent == 0) 
                return 0;

            var snapshot = await _localDbManager.LoadSnapshot();
            if (snapshot == null) return -1;
            if (snapshot.Length == 0)
                snapshot = await DownloadSnapshot();
            if (snapshot == null) return -1;

            if (!await _readModel.Deserialize(_logFile, snapshot))
                return -1;

            return _currentDatacenterParameters.SnapshotLastEvent;
        }

        private async Task<int> LoadAndApplyEvents(int lastLoadedEvent)
        {
            var jsonsInCache = await _localDbManager.LoadEvents();
            _logFile.AppendLine($@"{jsonsInCache.Length} events in cache should be applying...");
            return ApplyBatch(jsonsInCache);
        }

//        public async Task<int> Load()
//        {
//            var currentEventNumber = await LoadFromCache();
//            currentEventNumber = await DownloadEvents(currentEventNumber);
//            // some sort of parsing snapshot
//            _renderingManager.Initialize();
//            await _renderingManager.RenderCurrentZoneOnApplicationStart();
//            return currentEventNumber;
//        }

//        private async Task<int> LoadFromCache()
//        {
//            if (_currentDatacenterParameters.SnapshotLastEvent != 0)
//            {
//                var snapshot = await _localDbManager.LoadSnapshot();
//                if (snapshot == null) return -1;
//                if (snapshot.Length == 0)
//                    snapshot = await DownloadSnapshot();
//                if (snapshot == null) return -1;
//                if (!await _readModel.Deserialize(_logFile, snapshot))
//                    return -1;
//            }
//
//            var jsonsInCache = await _localDbManager.LoadEvents();
//            _logFile.AppendLine($@"{jsonsInCache.Length} events in cache should be applying...");
//            return ApplyBatch(jsonsInCache);
//        }

        private async Task<byte[]> DownloadSnapshot()
        {
            try
            {
                _logFile.AppendLine($@"Snapshot with last event number {_currentDatacenterParameters.SnapshotLastEvent} not found in cache, downloading...");
                var dto = await _c2DWcfManager.GetSnapshotParams(
                    new GetSnapshotDto() { LastIncludedEvent = _currentDatacenterParameters.SnapshotLastEvent });
                _logFile.AppendLine($@"{dto.PortionsCount} portions in snapshot");
                if (dto.PortionsCount < 1)
                    return null;

                var snapshot = new byte[dto.Size];
                var offset = 0;
                for (int i = 0; i < dto.PortionsCount; i++)
                {
                    var portion = await _c2DWcfManager.GetSnapshotPortion(i);
                    var unused = await _localDbManager.SaveSnapshot(portion);
                    portion.CopyTo(snapshot, offset);
                    offset = offset + portion.Length;
                    _logFile.AppendLine($@"portion {i}  {portion.Length} bytes received and saved in cache");
                }

                return snapshot;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"DownloadSnapshot : {e.Message}");
                return null;
            }
        }

        private async Task<int> DownloadAndApplyEvents(int currentEventNumber)
        {
            string[] events;
            do
            {
                events = await _c2DWcfManager.GetEvents(new GetEventsDto() { Revision = currentEventNumber });
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
                var msg = (EventMessage)JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                var evnt = msg.Body;

                try
                {
                    _readModel.Apply(evnt);
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
                        $@"Exception thrown while processing event with timestamp {msg.Headers[header]} \n {evnt.GetType().FullName}");
                }
            }

            return events.Length;
        }

    }
}