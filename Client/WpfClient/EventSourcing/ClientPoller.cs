using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;
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
        public IWcfServiceForClient Channel;
        private readonly OpticalEventsViewModel _opticalEventsViewModel;
        private readonly NetworkEventsViewModel _networkEventsViewModel;
        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        public List<object> ReadModels { get; }

        public int CurrentEventNumber { get; private set; }

        public int LastOpticalEventNumber { get; set; }
        public int LastNetworkEventNumber { get; set; }

        public ClientPoller(IWcfServiceForClient channel, List<object> readModels,
            OpticalEventsViewModel opticalEventsViewModel, NetworkEventsViewModel networkEventsViewModel,
            IMyLog logFile, ILocalDbManager localDbManager)
        {
            Channel = channel;
            _opticalEventsViewModel = opticalEventsViewModel;
            _networkEventsViewModel = networkEventsViewModel;
            _logFile = logFile;
            _localDbManager = localDbManager;
            ReadModels = readModels;
        }

        public void LoadCache(string serverAddress)
        {
            ((LocalDbManager)_localDbManager).Initialize(serverAddress);
            var jsonsInCache = _localDbManager.LoadEvents();
            ApplyEventSourcingEvents(jsonsInCache);
        }

        public void Tick()
        {
            string[] events = Channel.GetEvents(CurrentEventNumber);// .Result;
            if (events == null)
            {
                _logFile.AppendLine(@"Cannot establish datacenter connection.");
                return;
            }

            if (events.Length > 0)
            {
                _localDbManager.SaveEvents(events);
                ApplyEventSourcingEvents(events);
            }


            var opticalEvents = Channel.GetOpticalEvents(LastOpticalEventNumber).Result;
            if (opticalEvents?.Events != null && opticalEvents.Events.Any())
            {
                LastOpticalEventNumber = opticalEvents.Events.Last().Id;
                foreach (var opticalEvent in opticalEvents.Events)
                {
                    _opticalEventsViewModel.Apply(opticalEvent);
                }
            }

            var networkEvents = Channel.GetNetworkEvents(LastNetworkEventNumber).Result;
            if (networkEvents?.Events != null && networkEvents.Events.Any())
            {
                LastNetworkEventNumber = networkEvents.Events.Last().Id;
                foreach (var networkEvent in networkEvents.Events)
                {
                    _networkEventsViewModel.Apply(networkEvent);
                }
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
    }
}