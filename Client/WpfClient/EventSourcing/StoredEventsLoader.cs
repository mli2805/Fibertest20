using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;
using Newtonsoft.Json;
using NEventStore;

namespace Iit.Fibertest.Client
{
    public class LoadingResult
    {
        public int Count;
        public DateTime LastEventTimestamp;
    }

    public enum CacheClearResult { FailedToClear, CacheNotFound, ClearedSuccessfully, CacheMatchesDb }
    public class StoredEventsLoader
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        const string Header = @"Timestamp";

        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly IWcfServiceInSuperClient _c2SWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly CommandLineParameters _commandLineParameters;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly ServerConnectionLostViewModel _serverConnectionLostViewModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly SnapshotsLoader _snapshotsLoader;
        private readonly EventsOnTreeExecutor _eventsOnTreeExecutor;
        private readonly OpticalEventsExecutor _opticalEventsExecutor;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly RenderingManager _renderingManager;

        public StoredEventsLoader(IMyLog logFile, ILocalDbManager localDbManager, IWcfServiceInSuperClient c2SWcfManager,
            IWindowManager windowManager, IDispatcherProvider dispatcherProvider,
            CommandLineParameters commandLineParameters, CurrentDatacenterParameters currentDatacenterParameters,
            ServerConnectionLostViewModel serverConnectionLostViewModel, IWcfServiceDesktopC2D c2DWcfManager, CurrentUser currentUser,
            Model readModel, SnapshotsLoader snapshotsLoader,
            EventsOnTreeExecutor eventsOnTreeExecutor,
            OpticalEventsExecutor opticalEventsExecutor,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            RenderingManager renderingManager)
        {
            _logFile = logFile;
            _localDbManager = localDbManager;
            _c2SWcfManager = c2SWcfManager;
            _windowManager = windowManager;
            _dispatcherProvider = dispatcherProvider;
            _commandLineParameters = commandLineParameters;
            _currentDatacenterParameters = currentDatacenterParameters;
            _serverConnectionLostViewModel = serverConnectionLostViewModel;
            _c2DWcfManager = c2DWcfManager;
            _currentUser = currentUser;
            _readModel = readModel;
            _snapshotsLoader = snapshotsLoader;
            _eventsOnTreeExecutor = eventsOnTreeExecutor;
            _opticalEventsExecutor = opticalEventsExecutor;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _renderingManager = renderingManager;
        }

        public async Task<CacheClearResult> ClearCacheIfDoesnotMatchDb()
        {
            var cacheParameters = await _localDbManager.GetCacheParameters();
            if (cacheParameters == null || cacheParameters.LastEventNumber == 0) return CacheClearResult.CacheNotFound;

            if (cacheParameters.SnapshotLastEventNumber != _currentDatacenterParameters.SnapshotLastEvent
                || !await CompareLastEvent(cacheParameters.LastEventNumber, cacheParameters.LastEventTimestamp))
            {
                return await _localDbManager.RecreateCacheDb() ? CacheClearResult.ClearedSuccessfully : CacheClearResult.FailedToClear;
            }

            return CacheClearResult.CacheMatchesDb;
        }

        private async Task<bool> CompareLastEvent(int count, DateTime lastEventTimestamp)
        {
            var compareEventDto = new CompareEventDto()
            {
                ConnectionId = _currentUser.ConnectionId,
                Revision = count - 1,
                Timestamp = lastEventTimestamp,
            };

            if (await _c2DWcfManager.CompareEvent(compareEventDto)) return true;

            _logFile.AppendLine(@"Cache does not match server's database!");
            return false;
        }

        public async Task<int> TwoComponentLoading(bool isCleared)
        {
            try
            {
                var lastEventFromSnapshot = await _snapshotsLoader.LoadAndApplySnapshot(isCleared);

                var loadingCacheResult = await LoadAndApplyEventsFromCache(lastEventFromSnapshot);
                loadingCacheResult.Count += lastEventFromSnapshot;
                _logFile.AppendLine($@"Last loaded from cache event has timestamp {loadingCacheResult.LastEventTimestamp:O}");
                var currentEventNumber = await DownloadAndApplyEvents(loadingCacheResult);

                _renderingManager.Initialize();
                await _renderingManager.RenderCurrentZoneOnApplicationStart();
                return currentEventNumber;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"TwoComponentLoading: {e.Message}");
                _dispatcherProvider.GetDispatcher().Invoke(NotifyUserConnectionProblems); // blocks current thread till user clicks to close form
                return -1;
            }
        }

        private async Task<LoadingResult> LoadAndApplyEventsFromCache(int lastLoadedEvent)
        {
            var jsonsInCache = await _localDbManager.LoadEvents(lastLoadedEvent);
            _logFile.AppendLine($@"{jsonsInCache.Length} events in cache should be applied...");
            var result = new LoadingResult { Count = ApplyBatch(jsonsInCache) };
            if (jsonsInCache.Any())
            {
                var json = jsonsInCache.Last();
                var msg = (EventMessage)JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                result.LastEventTimestamp = (DateTime)msg.Headers[Header];
            }
            return result;
        }

        private async Task<int> DownloadAndApplyEvents(LoadingResult cacheLoadingResult)
        {
            var currentEventNumber = cacheLoadingResult.Count;
            _logFile.AppendLine($@"Downloading events from {currentEventNumber}...");
            int logged = currentEventNumber;
            string[] events;
            do
            {
                events = await _c2DWcfManager.GetEvents(new GetEventsDto() { Revision = currentEventNumber, ConnectionId = _currentUser.ConnectionId });
                await _localDbManager.SaveEvents(events, currentEventNumber + 1);
                currentEventNumber = currentEventNumber + ApplyBatch(events);
                if (currentEventNumber - logged >= 1000)
                {
                    _logFile.AppendLine($@"{currentEventNumber}...");
                    logged = currentEventNumber;
                }
            } while (events.Length != 0);

            _logFile.AppendLine($@"{currentEventNumber} is last event number found in Cache + Db");
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
                    _logFile.AppendLine(
                        $@"Exception thrown while processing event with timestamp {msg.Headers[Header]} \n {
                                evnt.GetType().FullName
                            }");
                }

                if (i > 0 && i % 8000 == 0)
                    _logFile.AppendLine($@"{i}...");
            }
            return events.Length;
        }

        private void NotifyUserConnectionProblems()
        {
            _serverConnectionLostViewModel.Initialize(_currentDatacenterParameters.ServerTitle, _currentDatacenterParameters.ServerIp);
            _serverConnectionLostViewModel.PropertyChanged += OnServerConnectionLostViewModelOnPropertyChanged;
            _windowManager.ShowDialogWithAssignedOwner(_serverConnectionLostViewModel);
        }

        private void OnServerConnectionLostViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"IsOpen")
            {
                if (_commandLineParameters.IsUnderSuperClientStart)
                    _c2SWcfManager.NotifyConnectionBroken(_commandLineParameters.ClientOrdinal);
                Application.Current.Shutdown();
            }
        }
    }
}