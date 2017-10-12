using System;
using System.Linq;
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


        public string SendCommand(object cmd)
        {
            var result = (string)_aggregate.AsDynamic().When(cmd);
            if (IsSuccess(result))
            {
                var eventStream = _storeEvents.OpenStream(AggregateId);
                foreach (var e in WriteModel.EventsWaitingForCommit)
                    eventStream.Add(new EventMessage { Body = e });
                WriteModel.Commit();
                eventStream.CommitChanges(Guid.NewGuid());
            }
            return result;
        }

        public string SendCommand(string json)
        {
            var cmd = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
            var result = (string)_aggregate.AsDynamic().When(cmd);
            if (IsSuccess(result))
            {
                var eventStream = _storeEvents.OpenStream(AggregateId);
                foreach (var e in WriteModel.EventsWaitingForCommit)
                    eventStream.Add(new EventMessage { Body = e });
                WriteModel.Commit();
                eventStream.CommitChanges(Guid.NewGuid());
            }
            return result;
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
            catch (StreamNotFoundException e)
            {
                _logFile.AppendLine(e.Message);
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