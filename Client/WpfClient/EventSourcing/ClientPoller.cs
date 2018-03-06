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
        private readonly EventsOnModelExecutor _eventsOnModelExecutor;
        private Thread _pollerThread;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly int _pollingRate;
        public CancellationToken CancellationToken { get; set; }

        public int CurrentEventNumber { get; private set; }

        public ClientPoller(IWcfServiceForClient wcfConnection, IDispatcherProvider dispatcherProvider,
            ReadModel readModel, TreeOfRtuModel treeOfRtuModel, EventsOnGraphExecutor eventsOnGraphExecutor, EventsOnModelExecutor eventsOnModelExecutor,
            IMyLog logFile, IniFile iniFile, ILocalDbManager localDbManager)
        {
            _wcfConnection = wcfConnection;
            _readModel = readModel;
            _treeOfRtuModel = treeOfRtuModel;
            _eventsOnGraphExecutor = eventsOnGraphExecutor;
            _eventsOnModelExecutor = eventsOnModelExecutor;
            _dispatcherProvider = dispatcherProvider;
            _logFile = logFile;
            _localDbManager = localDbManager;
            _pollingRate = iniFile.Read(IniSection.General, IniKey.ClientPollingRateMs, 500);
        }

     
        public async Task<int> LoadEventSourcingCache()
        {
            var jsonsInCache = await _localDbManager.LoadEvents();
            ApplyEventSourcingEvents(jsonsInCache);
            return CurrentEventNumber;
        }

        public async Task LoadEventSourcingDb()
        {
            string[] events;
            do
            {
                events = await _wcfConnection.GetEvents(CurrentEventNumber);
                await _localDbManager.SaveEvents(events);
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
                    _eventsOnGraphExecutor.Apply(evnt);
                    _eventsOnModelExecutor.Apply(evnt);
                    _treeOfRtuModel.AsDynamic().Apply(evnt);

                    // some forms refresh their view because they have sent command previously and are waiting event's coming
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