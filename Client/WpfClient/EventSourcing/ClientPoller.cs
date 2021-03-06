﻿using System;
using System.ComponentModel;
using System.Threading;
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
    public class ClientPoller : PropertyChangedBase
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        private readonly IWcfServiceDesktopC2D _wcfConnection;
        private readonly IWindowManager _windowManager;
        private readonly Model _readModel;
        private readonly ServerConnectionLostViewModel _serverConnectionLostViewModel;
        private readonly IWcfServiceInSuperClient _c2SWcfManager;
        private readonly SystemState _systemState;
        private readonly CurrentUser _currentUser;
        private readonly CommandLineParameters _commandLineParameters;
        private readonly EventsOnGraphExecutor _eventsOnGraphExecutor;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly EventsOnTreeExecutor _eventsOnTreeExecutor;
        private readonly OpticalEventsExecutor _opticalEventsExecutor;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly RtuChannelViewsManager _rtuChannelViewsManager;
        private readonly BopStateViewsManager _bopStateViewsManager;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly LandmarksViewsManager _landmarksViewsManager;
        private Thread _pollerThread;
        private readonly IDispatcherProvider _dispatcherProvider;
        private int _exceptionCount;
        private readonly int _exceptionCountLimit;
        private readonly IMyLog _logFile;
        private readonly EventArrivalNotifier _eventArrivalNotifier;
        private readonly ILocalDbManager _localDbManager;
        private readonly int _pollingRate;
        public CancellationTokenSource CancellationTokenSource { get; set; }

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

        public ClientPoller(IWcfServiceDesktopC2D wcfConnection, IDispatcherProvider dispatcherProvider, 
            IWindowManager windowManager, Model readModel,
            ServerConnectionLostViewModel serverConnectionLostViewModel, 
            IWcfServiceInSuperClient c2SWcfManager, SystemState systemState, CurrentUser currentUser,
            CommandLineParameters commandLineParameters, CurrentDatacenterParameters currentDatacenterParameters, 

            EventsOnGraphExecutor eventsOnGraphExecutor, 
            EventsOnTreeExecutor eventsOnTreeExecutor, OpticalEventsExecutor opticalEventsExecutor,

            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
            RtuStateViewsManager rtuStateViewsManager, RtuChannelViewsManager rtuChannelViewsManager,
            BopStateViewsManager bopStateViewsManager, NetworkEventsDoubleViewModel networkEventsDoubleViewModel, 
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel, LandmarksViewsManager landmarksViewsManager,

            IMyLog logFile, IniFile iniFile, EventArrivalNotifier eventArrivalNotifier, ILocalDbManager localDbManager)
        {
            _wcfConnection = wcfConnection;
            _windowManager = windowManager;
            _readModel = readModel;
            _serverConnectionLostViewModel = serverConnectionLostViewModel;
            _c2SWcfManager = c2SWcfManager;
            _systemState = systemState;
            _currentUser = currentUser;
            _commandLineParameters = commandLineParameters;
            _eventsOnGraphExecutor = eventsOnGraphExecutor;
            _currentDatacenterParameters = currentDatacenterParameters;
            _eventsOnTreeExecutor = eventsOnTreeExecutor;
            _opticalEventsExecutor = opticalEventsExecutor;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _rtuStateViewsManager = rtuStateViewsManager;
            _rtuChannelViewsManager = rtuChannelViewsManager;
            _bopStateViewsManager = bopStateViewsManager;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _landmarksViewsManager = landmarksViewsManager;
            _dispatcherProvider = dispatcherProvider;
            _logFile = logFile;
            _eventArrivalNotifier = eventArrivalNotifier;
            _localDbManager = localDbManager;
            _pollingRate = iniFile.Read(IniSection.General, IniKey.ClientPollingRateMs, 500);
            _exceptionCountLimit = iniFile.Read(IniSection.General, IniKey.FailedPollsLimit, 7);
        }

        public void Start()
        {
            _logFile.AppendLine(@"Polling started");
            _pollerThread = new Thread(DoPolling) { IsBackground = true };
            _pollerThread.Start();
        }

        private async void DoPolling()
        {
            if (_commandLineParameters.IsUnderSuperClientStart)
                _systemState.PropertyChanged += _systemState_PropertyChanged;
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                await EventSourcingTick();
                Thread.Sleep(TimeSpan.FromMilliseconds(_pollingRate));
            }
            _logFile.AppendLine(@"Leaving DoPolling...");
        }

        private void _systemState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _logFile.AppendLine(@"Notify super-client system state changed.");
            _c2SWcfManager.SetSystemState(_commandLineParameters.ClientOrdinal, !_systemState.HasAnyActualProblem);
        }

        public async Task<int> EventSourcingTick()
        {
            string[] events = await _wcfConnection.GetEvents(
                new GetEventsDto(){Revision = CurrentEventNumber, ConnectionId = _currentUser.ConnectionId});

            if (events == null)
            {
                _exceptionCount++;
                _logFile.AppendLine($@"Cannot establish connection with data-center. Exception count: {_exceptionCount}");
                if (_exceptionCount == _exceptionCountLimit) // blocks current thread till user clicks to close form
                    _dispatcherProvider.GetDispatcher().Invoke(NotifyUserConnectionProblems); 
                return -1;
            }

            _exceptionCount = 0;

            if (events.Length == 0)
                return 0;

            await _localDbManager.SaveEvents(events, CurrentEventNumber + 1);
            _dispatcherProvider.GetDispatcher().Invoke(() => ApplyEventSourcingEvents(events)); // sync, GUI thread
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

        private void ApplyEventSourcingEvents(string[] events)
        {
            foreach (var json in events)
            {
                var msg = (EventMessage)JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                if (msg == null) continue;
                var evnt = msg.Body;

                try
                {
                    _readModel.Apply(evnt);

                    _eventsOnGraphExecutor.Apply(evnt);
                    _eventsOnTreeExecutor.Apply(evnt);
                    _opticalEventsExecutor.Apply(evnt);
                    _networkEventsDoubleViewModel.Apply(evnt);
                    _rtuStateViewsManager.Apply(evnt);
                    _traceStateViewsManager.Apply(evnt);
                    _traceStatisticsViewsManager.Apply(evnt);
                    _landmarksViewsManager.Apply(evnt);
                    _rtuChannelViewsManager.Apply(evnt);
                    _bopStateViewsManager.Apply(evnt);
                    _bopNetworkEventsDoubleViewModel.Apply(evnt);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                    var header = @"Timestamp";
                    _logFile.AppendLine($@"Exception thrown while processing event with timestamp {msg.Headers[header]} \n {evnt.GetType().FullName}");
                }

                CurrentEventNumber++;
            }
        }
    }
}