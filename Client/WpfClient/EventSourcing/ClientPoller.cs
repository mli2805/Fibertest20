using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Client
{
    public interface IDispatcherProvider { Dispatcher GetDispatcher(); }
    public class UiDispatcherProvider : IDispatcherProvider
    {
        public Dispatcher GetDispatcher() { return Application.Current.Dispatcher; }
    }

    public class TestsDispatcherProvider : IDispatcherProvider
    {
        public Dispatcher GetDispatcher() { return Dispatcher.CurrentDispatcher; }
    }

    public class ClientPoller : PropertyChangedBase
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        public IWcfServiceForClient WcfConnection;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly OpticalEventsViewModel _opticalEventsViewModel;
        private readonly NetworkEventsViewModel _networkEventsViewModel;
        private readonly IMyLog _logFile;
        private readonly IniFile _iniFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly int _pollingRate;
        public List<object> ReadModels { get; }

        public int CurrentEventNumber { get; private set; }

        public int LastOpticalEventNumber { get; set; }
        public int LastNetworkEventNumber { get; set; }

        public ClientPoller(IWcfServiceForClient wcfConnection, List<object> readModels, IDispatcherProvider dispatcherProvider,
            OpticalEventsViewModel opticalEventsViewModel, NetworkEventsViewModel networkEventsViewModel,
            IMyLog logFile, IniFile iniFile, ILocalDbManager localDbManager)
        {
            WcfConnection = wcfConnection;
            _dispatcherProvider = dispatcherProvider;
            _opticalEventsViewModel = opticalEventsViewModel;
            _networkEventsViewModel = networkEventsViewModel;
            _logFile = logFile;
            _iniFile = iniFile;
            _localDbManager = localDbManager;
            ReadModels = readModels;
            _pollingRate = _iniFile.Read(IniSection.General, IniKey.ClientPollingRate, 1);
        }

        public void LoadEventSourcingCache(string serverAddress)
        {
            ((LocalDbManager)_localDbManager).Initialize(serverAddress);
            var jsonsInCache = _localDbManager.LoadEvents();
            ApplyEventSourcingEvents(jsonsInCache);
        }

        public void Start()
        {
            var pollerThread = new Thread(Cycle) { IsBackground = true };
            pollerThread.Start();
        }

        // ReSharper disable once FunctionNeverReturns
        private void Cycle()
        {
            while (true)
            {
                EventSourcingTick();
//                OpticalEventsTick();
//                NetworkEventsTick();
                Thread.Sleep(TimeSpan.FromSeconds(_pollingRate));
            }
        }

        public void EventSourcingTick()
        {
            string[] events = WcfConnection.GetEvents(CurrentEventNumber);// .Result;
            if (events == null)
            {
                _logFile.AppendLine(@"Cannot establish datacenter connection.");
                return;
            }

            if (events.Length > 0)
            {
                _localDbManager.SaveEvents(events);

//                Application.Current.Dispatcher.Invoke(() => ApplyEventSourcingEvents(events));
//                Dispatcher.CurrentDispatcher.Invoke(() => ApplyEventSourcingEvents(events));
                _dispatcherProvider.GetDispatcher().Invoke(() => ApplyEventSourcingEvents(events));
                //ApplyEventSourcingEvents(events);
            }
        }

        private void ApplyEventSourcingEvents(string[] events)
        {
            foreach (var json in events)
            {
                var e = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                foreach (var m in ReadModels)
                {
                    m.AsDynamic().Apply(e);

                    //
                    var readModel = m as ReadModel;
                    readModel?.NotifyOfPropertyChange(nameof(readModel.JustForNotification));
                    //
                    var treeModel = m as TreeOfRtuModel;
                    treeModel?.NotifyOfPropertyChange(nameof(treeModel.Statistics));
                }
                CurrentEventNumber++;
            }
        }

        private void OpticalEventsTick()
        {
            var opticalEvents = WcfConnection.GetOpticalEvents(LastOpticalEventNumber).Result;
            if (opticalEvents?.Measurements != null && opticalEvents.Measurements.Any())
            {
                ApplyOpticalEvents(opticalEvents);

                LastOpticalEventNumber = opticalEvents.Measurements.Last().Id;
                foreach (var opticalEvent in opticalEvents.Measurements)
                {
                    _opticalEventsViewModel.Apply(opticalEvent);
                }
            }
        }

        private void NetworkEventsTick()
        {
            var networkEvents = WcfConnection.GetNetworkEvents(LastNetworkEventNumber).Result;
            if (networkEvents?.Events != null && networkEvents.Events.Any())
            {
                ApplyNetworkEvents(networkEvents);

                LastNetworkEventNumber = networkEvents.Events.Last().Id;
                foreach (var networkEvent in networkEvents.Events)
                {
                    _networkEventsViewModel.Apply(networkEvent);
                }
            }
        }

        private void ApplyNetworkEvents(NetworkEventsList list)
        {
            foreach (var networkEvent in list.Events)
            {
                foreach (var m in ReadModels)
                {
                    m.AsDynamic().Apply(networkEvent);
                }
            }
        }
        private void ApplyOpticalEvents(MeasurementsList list)
        {
            foreach (var opticalEvent in list.Measurements)
            {
                foreach (var m in ReadModels)
                {
                    m.AsDynamic().Apply(opticalEvent);
                }
            }
        }
    }
}