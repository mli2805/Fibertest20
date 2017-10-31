using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;
using NEventStore;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.DataCenterCore
{
    //TODO: Either merge projects, or use an interface
    public class EventStoreService
    {
        private readonly IMyLog _logFile;
        private readonly IEventStoreInitializer _eventStoreInitializer;
        private IStoreEvents _storeEvents;
        private Aggregate _aggregate;
        private static readonly Guid AggregateId =
            new Guid("1C28CBB5-A9F5-4A5C-B7AF-3D188F8F24ED");

        public WriteModel WriteModel;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public EventStoreService(IMyLog logFile, IEventStoreInitializer eventStoreInitializer)
        {
            _logFile = logFile;
            _eventStoreInitializer = eventStoreInitializer;
        }

        public void Init()
        {
            _storeEvents = _eventStoreInitializer.Init(_logFile);

            var eventStream = _storeEvents.OpenStream(AggregateId);
            var events = eventStream.CommittedEvents.Select(x => x.Body);

            WriteModel = new WriteModel(events);
            _aggregate = new Aggregate(WriteModel);
        }


//        public Task<string> SendCommand(string json)
//        {
//            var cmd = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
//            return SendCommand(cmd);
//        }

        public Task<string> SendCommand(object cmd)
        {
            var result = (string)_aggregate.AsDynamic().When(cmd); // Aggregate checks if command is valid
                                                                   // and if so transforms command into event and passes it to WriteModel
                                                                   // WriteModel applies event

            if (IsSuccess(result))                                   // if command was valid
            {
                var eventStream = _storeEvents.OpenStream(AggregateId);  
                foreach (var e in WriteModel.EventsWaitingForCommit)   // takes already applied event from WriteModel's list
                    eventStream.Add(new EventMessage { Body = e });   // and stores this event in BD
                WriteModel.Commit();                                     // now cleans WriteModel's list
                eventStream.CommitChanges(Guid.NewGuid());
            }
            return Task.FromResult(result);
        }

        private static bool IsSuccess(string result)
        {
            // TODO: Make sure this is correct
            return string.IsNullOrEmpty(result);
        }

        public string[] GetEvents(int revision)
        {
            try
            {
                return _storeEvents
                    .OpenStream(AggregateId, revision + 1)
                    .CommittedEvents
                    .Select(x => x.Body)
                    .Select(x => JsonConvert.SerializeObject(x, JsonSerializerSettings))
                    .ToArray();
            }
            catch (StreamNotFoundException)
            {
                return new string[0];
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new string[0];
            }
        }
    }
}