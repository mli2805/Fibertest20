using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
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
        private readonly ReadModel _readModel;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly EventsOnGraphExecutor _eventsOnGraphExecutor;
        private readonly EventsOnModelExecutor _eventsOnModelExecutor;
        private readonly EventsOnTreeExecutor _eventsOnTreeExecutor;
        private readonly OpticalEventsExecutor _opticalEventsExecutor;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private Thread _pollerThread;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly int _pollingRate;
        public CancellationToken CancellationToken { get; set; }

        public int CurrentEventNumber { get; set; }

        public ClientPoller(IWcfServiceForClient wcfConnection, IDispatcherProvider dispatcherProvider,
            ReadModel readModel, TreeOfRtuModel treeOfRtuModel, EventsOnGraphExecutor eventsOnGraphExecutor,
            EventsOnModelExecutor eventsOnModelExecutor, EventsOnTreeExecutor eventsOnTreeExecutor, OpticalEventsExecutor opticalEventsExecutor,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel, RtuStateViewsManager rtuStateViewsManager,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            IMyLog logFile, IniFile iniFile, ILocalDbManager localDbManager)
        {
            _wcfConnection = wcfConnection;
            _readModel = readModel;
            _treeOfRtuModel = treeOfRtuModel;
            _eventsOnGraphExecutor = eventsOnGraphExecutor;
            _eventsOnModelExecutor = eventsOnModelExecutor;
            _eventsOnTreeExecutor = eventsOnTreeExecutor;
            _opticalEventsExecutor = opticalEventsExecutor;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _rtuStateViewsManager = rtuStateViewsManager;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _dispatcherProvider = dispatcherProvider;
            _logFile = logFile;
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
            string[] events = await _wcfConnection.GetEvents(CurrentEventNumber);
            if (events == null)
            {
                _logFile.AppendLine(@"Cannot establish connection with data-center.");
                return 0;
            }

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

                    if (evnt is MeasurementAdded mee)
                    {
                        _traceStateViewsManager.AddMeasurement(mee);
                        _traceStatisticsViewsManager.AddMeasurement(mee);
                    }
                    if (evnt is MeasurementUpdated mue)
                    {
                        _traceStateViewsManager.UpdateMeasurement(mue);
                        _traceStatisticsViewsManager.UpdateMeasurement(mue);
                    }
                    // TODO both forms should react TraceUpdated (Title)


                    if (evnt is BopNetworkEventAdded bee)
                        _bopNetworkEventsDoubleViewModel.Apply(bee);

                    // some forms refresh their view because they have sent command previously and are waiting event's arrival
                    _readModel.NotifyOfPropertyChange(nameof(_readModel.JustForNotification));

                    // otherwise I should do this in almost all operations of applying events in tree
                    _treeOfRtuModel.NotifyOfPropertyChange(nameof(_treeOfRtuModel.Statistics));
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