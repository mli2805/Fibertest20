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
        private readonly WriteModel _writeModel;
        private IStoreEvents _storeEvents;
        private Aggregate _aggregate;
        private static readonly Guid AggregateId =
            new Guid("1C28CBB5-A9F5-4A5C-B7AF-3D188F8F24ED");

        private readonly int _eventsPortion;


        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public EventStoreService(IniFile iniFile, IMyLog logFile, IEventStoreInitializer eventStoreInitializer, WriteModel writeModel, Aggregate aggregate)
        {
            _eventsPortion = iniFile.Read(IniSection.General, IniKey.EventSourcingPortion, 100);
            _logFile = logFile;
            _eventStoreInitializer = eventStoreInitializer;
            _writeModel = writeModel;
            _aggregate = aggregate;
        }

        public void Init()
        {
            _storeEvents = _eventStoreInitializer.Init(_logFile);

            var eventStream = _storeEvents.OpenStream(AggregateId);
            var events = eventStream.CommittedEvents.Select(x => x.Body);

            _writeModel.Init(events);
            _logFile.AppendLine("All events from base are applied to WriteModel");
        }

        // ilya: can add user name\id\ip address as an argument here\client timestamp
        public Task<string> SendCommand(object cmd, string username)
        {
            // ilya: can pass user id\role as an argument to When to check permissions
            var result = (string)_aggregate.AsDynamic().When(cmd); // Aggregate checks if command is valid
                                                                   // and if so, transforms command into event and passes it to WriteModel
                                                                   // WriteModel applies event

            if (IsSuccess(result))                                   // if command was valid
            {
                var eventStream = _storeEvents.OpenStream(AggregateId);  
                foreach (var e in _writeModel.EventsWaitingForCommit)   // takes already applied event from WriteModel's list
                {
                    var msg = new EventMessage();
                    msg.Headers.Add("Timestamp", DateTime.Now);
                    msg.Headers.Add("Username", username);
                    msg.Body = e;
                    eventStream.Add(msg);   // and stores this event in BD
                }
                _writeModel.Commit();                                     // now cleans WriteModel's list
                eventStream.CommitChanges(Guid.NewGuid());
            }
            return Task.FromResult(result);
        }

        private static bool IsSuccess(string result)
        {
            // TODO: Make sure this is correct
            return result == null;
        }

        public string[] GetEvents(int revision)
        {
            try
            {
                var events = _storeEvents
                    .OpenStream(AggregateId, revision + 1)
                    .CommittedEvents
                    .Select(x => x.Body)
                    .Select(x => JsonConvert.SerializeObject(x, JsonSerializerSettings))
                    .Take(_eventsPortion) // it depends on tcp buffer size and performance of clients' pc
                    .ToArray();
                return events;
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