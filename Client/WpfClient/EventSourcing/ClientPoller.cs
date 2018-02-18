using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
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
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        private readonly IWcfServiceForClient _wcfConnection;
        private readonly ReadModel _readModel;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly EventsOnGraphExecutor _eventsOnGraphExecutor;
        private Thread _pollerThread;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly int _pollingRate;
        public CancellationToken CancellationToken { get; set; }

        public int CurrentEventNumber { get; private set; }

        public ClientPoller(IWcfServiceForClient wcfConnection, ReadModel readModel, TreeOfRtuModel treeOfRtuModel,
            EventsOnGraphExecutor eventsOnGraphExecutor, IDispatcherProvider dispatcherProvider,
            IMyLog logFile, IniFile iniFile, ILocalDbManager localDbManager)
        {
            _wcfConnection = wcfConnection;
            _readModel = readModel;
            _treeOfRtuModel = treeOfRtuModel;
            _eventsOnGraphExecutor = eventsOnGraphExecutor;
            _dispatcherProvider = dispatcherProvider;
            _logFile = logFile;
            _localDbManager = localDbManager;
            _pollingRate = iniFile.Read(IniSection.General, IniKey.ClientPollingRateMs, 500);
        }


        public void LoadEventSourcingCache(string serverAddress, Guid graphDbVersionId)
        {
            ((LocalDbManager)_localDbManager).Initialize(serverAddress, graphDbVersionId);
            var jsonsInCache = _localDbManager.LoadEvents();
            ApplyEventSourcingEvents(jsonsInCache);
            _logFile.AppendLine($@"{CurrentEventNumber} events found in cache");
        }

        public async Task LoadEventSourcingDb()
        {
            string[] events;
            do
            {
                events = await _wcfConnection.GetEvents(CurrentEventNumber);
                _localDbManager.SaveEvents(events);
                ApplyEventSourcingEvents(events);
            }
            while (events.Length != 0);
            _logFile.AppendLine($@"{CurrentEventNumber} events found in Db");
        }


        public void Start()
        {
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
                _eventsOnGraphExecutor.Apply(e);
                _readModel.AsDynamic().Apply(e);
                _treeOfRtuModel.AsDynamic().Apply(e);

                _readModel.NotifyOfPropertyChange(nameof(_readModel.JustForNotification));
                _treeOfRtuModel.NotifyOfPropertyChange(nameof(_treeOfRtuModel.Statistics));

                CurrentEventNumber++;
            }
        }
    }
}