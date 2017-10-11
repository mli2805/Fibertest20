using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.DataCenterCore
{
    //TODO: Either merge projects, or use an interface
    public class EventStoreService
    {
        private readonly IMyLog _logFile;
        private IStoreEvents _storeEvents;
        private Aggregate _aggregate;
        private static readonly Guid AggregateId =
            new Guid("1C28CBB5-A9F5-4A5C-B7AF-3D188F8F24ED");

        private WriteModel _writeModel;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public EventStoreService(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void Init()
        {
            _logFile.AppendLine(@"Initializing EventStoreService");

            try
            {
                _storeEvents = Wireup.Init()
                                        .UsingSqlPersistence("NEventStoreSQLite")
                                        .WithDialect(new SqliteDialect())
//                    .UsingInMemoryPersistence()
                    .InitializeStorageEngine()
                    // .WithPersistence()
                    .Build();

            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }

            var eventStream = _storeEvents.OpenStream(AggregateId);
            var events = eventStream.CommittedEvents.Select(x => x.Body);

            _writeModel = new WriteModel(events);
            _aggregate = new Aggregate(_writeModel);
        }


        public string SendCommand(object cmd)
        {
            var result = (string)_aggregate.AsDynamic().When(cmd);
            if (IsSuccess(result))
            {
                var eventStream = _storeEvents.OpenStream(AggregateId);
                foreach (var e in _writeModel.EventsWaitingForCommit)
                    eventStream.Add(new EventMessage { Body = e });
                _writeModel.Commit();
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
                foreach (var e in _writeModel.EventsWaitingForCommit)
                    eventStream.Add(new EventMessage { Body = e });
                _writeModel.Commit();
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
            catch (StreamNotFoundException)
            {
                return new string[0];
            }
        }
    }
}