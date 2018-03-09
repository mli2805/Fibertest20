using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;
using NEventStore;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.DataCenterCore
{
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
        public Guid GraphDbVersionId;

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
            _storeEvents = _eventStoreInitializer.Init();
            var eventStream = _storeEvents.OpenStream(AggregateId);

            if (!AssignGraphDbVersion(eventStream)) return;

            var events = eventStream.CommittedEvents.Select(x => x.Body).ToList();
            _writeModel.Init(events);

            _logFile.AppendLine("All events from base are applied to WriteModel");
        }

        private bool AssignGraphDbVersion(IEventStream eventStream)
        {
            var firstMessage = eventStream.CommittedEvents.FirstOrDefault();
            if (firstMessage == null)
            {
                GraphDbVersionId = Guid.NewGuid();
                Seed().Wait();
                _logFile.AppendLine("Empty graph is seeded with default zone and users.");
                return false;
            }

            if (!firstMessage.Headers.TryGetValue("VersionId", out object obj))
            {
                _logFile.AppendLine("Cannot get VersionId from graph database.");
                return false;
            }

            GraphDbVersionId = Guid.Parse((string)obj); // direct cast doesn't work
            return true;
        }

        private async Task<string> Seed()
        {
            await SendCommand(new AddZone() { IsDefaultZone = true, Title = StringResources.Resources.SID_Default_Zone }, "developer", "OnServer");

            await SendCommand(new AddUser() { UserId = Guid.NewGuid(), Title = "developer",   EncodedPassword = UserExt.FlipFlop("developer"),   Email = "", IsEmailActivated = false, Role = Role.Developer, IsDefaultZoneUser = true }, "developer", "OnServer");
            await SendCommand(new AddUser() { UserId = Guid.NewGuid(), Title = "root",        EncodedPassword = UserExt.FlipFlop("root"),        Email = "", IsEmailActivated = false, Role = Role.Root, IsDefaultZoneUser = true }, "developer", "OnServer");
            await SendCommand(new AddUser() { UserId = Guid.NewGuid(), Title = "operator",    EncodedPassword = UserExt.FlipFlop("operator"),    Email = "", IsEmailActivated = false, Role = Role.Operator, IsDefaultZoneUser = true }, "developer", "OnServer");
            await SendCommand(new AddUser() { UserId = Guid.NewGuid(), Title = "supervisor",  EncodedPassword = UserExt.FlipFlop("supervisor"),  Email = "", IsEmailActivated = false, Role = Role.Supervisor, IsDefaultZoneUser = true }, "developer", "OnServer");
            await SendCommand(new AddUser() { UserId = Guid.NewGuid(), Title = "superclient", EncodedPassword = UserExt.FlipFlop("superclient"), Email = "", IsEmailActivated = false, Role = Role.Superclient, IsDefaultZoneUser = true }, "developer", "OnServer");
            return null;
        }

        public Task<string> SendCommand(object cmd, string username, string clientIp)
        {
            // ilya: can pass user id\role as an argument to When to check permissions
            var result = (string)_aggregate.AsDynamic().When(cmd); // Aggregate checks if command is valid
                                                                   // and if so, transforms command into event and passes it to WriteModel
                                                                   // WriteModel applies event and returns whether event was applied successfully

            if (result == null)                                   // if command was valid and applied successfully it should be persisted
            {
                var eventStream = _storeEvents.OpenStream(AggregateId);
                foreach (var e in _writeModel.EventsWaitingForCommit)   // takes already applied event from WriteModel's list
                {
                    var msg = new EventMessage();
                    msg.Headers.Add("Timestamp", DateTime.Now);
                    msg.Headers.Add("Username", username);
                    msg.Headers.Add("ClientIp", clientIp);
                    msg.Headers.Add("VersionId", GraphDbVersionId);
                    msg.Body = e;
                    eventStream.Add(msg);   // and stores this event in BD
                }
                _writeModel.Commit();                                     // now cleans WriteModel's list
                eventStream.CommitChanges(Guid.NewGuid());
            }
            return Task.FromResult(result);
        }

        public string[] GetEvents(int revision)
        {
            try
            {
                var events = _storeEvents
                    .OpenStream(AggregateId, revision + 1)
                    .CommittedEvents
                    //     .Select(x => x.Body) // not only Body but Header too
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