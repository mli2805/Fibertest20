using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;
using NEventStore;

namespace Iit.Fibertest.Client
{
    public class ClientPoller : PropertyChangedBase
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        private readonly IWcfServiceForClient _wcfConnection;
        private readonly EventsOnGraphExecutor _eventsOnGraphExecutor;
        private readonly EventsOnModelExecutor _eventsOnModelExecutor;
        private readonly EventsOnTreeExecutor _eventsOnTreeExecutor;
        private readonly OpticalEventsExecutor _opticalEventsExecutor;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly LandmarksViewsManager _landmarksViewsManager;
        private Thread _pollerThread;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly IMyLog _logFile;
        private readonly EventArrivalNotifier _eventArrivalNotifier;
        private readonly ILocalDbManager _localDbManager;
        private readonly int _pollingRate;
        public CancellationToken CancellationToken { get; set; }

        private int _currentEventNumber;
        public int CurrentEventNumber
        {
            get => _currentEventNumber;
            set
            {
                _currentEventNumber = value;
                // some forms refresh their view because they have sent command previously and are waiting event's arrival
                _eventArrivalNotifier.NeverMind = _currentEventNumber;
            }
        }

        public ClientPoller(IWcfServiceForClient wcfConnection, IDispatcherProvider dispatcherProvider,
            EventsOnGraphExecutor eventsOnGraphExecutor,
            EventsOnModelExecutor eventsOnModelExecutor, EventsOnTreeExecutor eventsOnTreeExecutor, OpticalEventsExecutor opticalEventsExecutor,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel, RtuStateViewsManager rtuStateViewsManager,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel, LandmarksViewsManager landmarksViewsManager,
            IMyLog logFile, IniFile iniFile, EventArrivalNotifier eventArrivalNotifier, ILocalDbManager localDbManager)
        {
            _wcfConnection = wcfConnection;
            _eventsOnModelExecutor = eventsOnModelExecutor;
            _eventsOnGraphExecutor = eventsOnGraphExecutor;
            _eventsOnTreeExecutor = eventsOnTreeExecutor;
            _opticalEventsExecutor = opticalEventsExecutor;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _rtuStateViewsManager = rtuStateViewsManager;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _landmarksViewsManager = landmarksViewsManager;
            _dispatcherProvider = dispatcherProvider;
            _logFile = logFile;
            _eventArrivalNotifier = eventArrivalNotifier;
            _localDbManager = localDbManager;
            _pollingRate = iniFile.Read(IniSection.General, IniKey.ClientPollingRateMs, 500);
        }

        public void Start()
        {
            _logFile.AppendLine(@"Polling started");
            _pollerThread = new Thread(DoPolling) { IsBackground = true };
            _pollerThread.Start();
        }

        private async void DoPolling()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                await EventSourcingTick();
                Thread.Sleep(TimeSpan.FromMilliseconds(_pollingRate));
            }
        }

        public async Task<int> EventSourcingTick()
        {
            string[] events = await _wcfConnection.GetEvents(new GetEventsDto(){Revision = CurrentEventNumber});

            if (events == null)
            {
                _logFile.AppendLine(@"Cannot establish connection with data-center.");
                return -1;
            }

            if (events.Length == 0)
                return 0;

            await _localDbManager.SaveEvents(events);
            _dispatcherProvider.GetDispatcher().Invoke(() => ApplyEventSourcingEvents(events)); // sync, GUI thread
            return events.Length;
        }

        private void ApplyEventSourcingEvents(string[] events)
        {
            foreach (var json in events)
            {
                var msg = (EventMessage)JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                var evnt = msg.Body;

                try
                {
                    _eventsOnModelExecutor.Apply(evnt);

                    _eventsOnGraphExecutor.Apply(evnt);
                    _eventsOnTreeExecutor.Apply(evnt);
                    _opticalEventsExecutor.Apply(evnt);
                    _networkEventsDoubleViewModel.Apply(evnt);
                    _rtuStateViewsManager.Apply(evnt);
                    _traceStateViewsManager.Apply(evnt);
                    _traceStatisticsViewsManager.Apply(evnt);
                    _landmarksViewsManager.Apply(evnt);
                    _bopNetworkEventsDoubleViewModel.Apply(evnt);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                    var header = @"Timestamp";
                    _logFile.AppendLine($@"Exception thrown while processing event with timestamp {msg.Headers[header]}");
                }

                CurrentEventNumber++;
            }
        }
    }
}