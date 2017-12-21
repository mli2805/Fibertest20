using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;
using NEventStore;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Client
{
    public class ClientPoller : PropertyChangedBase
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        public IWcfServiceForClient WcfConnection;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly NetworkEventsViewModel _networkEventsViewModel;
        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly int _pollingRate;
        public List<object> ReadModels { get; }

        public int CurrentEventNumber { get; private set; }

        public int LastOpticalEventNumber { get; set; }
        public int LastNetworkEventNumber { get; set; }

        public ClientPoller(IWcfServiceForClient wcfConnection, List<object> readModels, IDispatcherProvider dispatcherProvider,
            NetworkEventsViewModel networkEventsViewModel,
            IMyLog logFile, IniFile iniFile, ILocalDbManager localDbManager)
        {
            WcfConnection = wcfConnection;
            _dispatcherProvider = dispatcherProvider;
            _networkEventsViewModel = networkEventsViewModel;
            _logFile = logFile;
            _localDbManager = localDbManager;
            ReadModels = readModels;
            _pollingRate = iniFile.Read(IniSection.General, IniKey.ClientPollingRateMs, 500);
        }

        public void LoadEventSourcingCache(string serverAddress)
        {
            ((LocalDbManager)_localDbManager).Initialize(serverAddress);
            var jsonsInCache = _localDbManager.LoadEvents();
            ApplyEventSourcingEvents(jsonsInCache);
        }

        public void Start()
        {
            var pollerThread = new Thread(DoPolling) { IsBackground = true };
            pollerThread.Start();
        }

        // ReSharper disable once FunctionNeverReturns
        private async void DoPolling()
        {
            while (true)
            {
                await EventSourcingTick();
                //                OpticalEventsTick();
                NetworkEventsTick();
                Thread.Sleep(TimeSpan.FromMilliseconds(_pollingRate));
            }
        }

        public async Task EventSourcingTick()
        {
            string[] events = await WcfConnection.GetEvents(CurrentEventNumber);
            if (events == null)
            {
                _logFile.AppendLine(@"Cannot establish datacenter connection.");
                return;
            }

            if (events.Length > 0)
            {
                _localDbManager.SaveEvents(events); // sync
                _dispatcherProvider.GetDispatcher().Invoke(() => ApplyEventSourcingEvents(events)); // sync
            }
        }

        private void ApplyEventSourcingEvents(string[] events)
        {
            foreach (var json in events)
            {
                var msg = (EventMessage)JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                var e = msg.Body;
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

        private void NetworkEventsTick()
        {
            var networkEvents = WcfConnection.GetNetworkEvents(LastNetworkEventNumber).Result;
            if (networkEvents?.Events == null || !networkEvents.Events.Any())
                return;

            _dispatcherProvider.GetDispatcher().Invoke(() => ApplyNetworkEvents(networkEvents));
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
            LastNetworkEventNumber = list.Events.Last().Id;
            foreach (var networkEvent in list.Events)
            {
                _networkEventsViewModel.Apply(networkEvent);
            }
        }
    }
}