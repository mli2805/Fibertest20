using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
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
        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly int _pollingRate;
        private List<object> ReadModels { get; }

        public int CurrentEventNumber { get; private set; }

        public ClientPoller(IWcfServiceForClient wcfConnection, List<object> readModels, IDispatcherProvider dispatcherProvider,
            IMyLog logFile, IniFile iniFile, ILocalDbManager localDbManager)
        {
            WcfConnection = wcfConnection;
            _dispatcherProvider = dispatcherProvider;
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
            _logFile.AppendLine($@"{CurrentEventNumber} events found in cache");
        }

        public async Task LoadEventSourcingDb()
        {
            string[] events;
            do
            {
                events = await WcfConnection.GetEvents(CurrentEventNumber);
                _localDbManager.SaveEvents(events);
                ApplyEventSourcingEvents(events);
            }
            while (events.Length != 0);
            _logFile.AppendLine($@"{CurrentEventNumber} events found in Db");
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
                Thread.Sleep(TimeSpan.FromMilliseconds(_pollingRate));
            }
        }

        public async Task<int> EventSourcingTick()
        {
            string[] events = await WcfConnection.GetEvents(CurrentEventNumber);
            if (events == null)
            {
                _logFile.AppendLine(@"Cannot establish datacenter connection.");
                return 0;
            }

            _localDbManager.SaveEvents(events); // sync
            _dispatcherProvider.GetDispatcher().Invoke(() => ApplyEventSourcingEvents(events)); // sync, GUI thread
            return events.Length;
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
    }
}