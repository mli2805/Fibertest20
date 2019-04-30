﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;
using NEventStore;

namespace Iit.Fibertest.DataCenterCore
{
    public class EventStoreService
    {
        private readonly IMyLog _logFile;
        private readonly IEventStoreInitializer _eventStoreInitializer;
        private readonly SnapshotRepository _snapshotRepository;
        private IStoreEvents _storeEvents;
        private readonly CommandAggregator _commandAggregator;
        private readonly EventsQueue _eventsQueue;
        private Model _writeModel;

        public Guid AggregateId;

        private readonly int _eventsPortion;
        public int LastEventNumberInSnapshot;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public EventStoreService(IniFile iniFile, IMyLog logFile, IEventStoreInitializer eventStoreInitializer,
            SnapshotRepository snapshotRepository,
             CommandAggregator commandAggregator, EventsQueue eventsQueue, Model writeModel)
        {
            _eventsPortion = iniFile.Read(IniSection.General, IniKey.EventSourcingPortion, 100);
            AggregateId = Guid.Parse(iniFile.Read(IniSection.General, IniKey.EventSourcingAggregateId, "1C28CBB5-A9F5-4A5C-B7AF-3D188F8F24ED"));
            _logFile = logFile;
            _eventStoreInitializer = eventStoreInitializer;
            _snapshotRepository = snapshotRepository;
            _commandAggregator = commandAggregator;
            _eventsQueue = eventsQueue;
            _writeModel = writeModel;
        }

        public async Task<int> Init()
        {
            _logFile.AppendLine($"Event store service AggregateId {AggregateId}");
            _storeEvents = _eventStoreInitializer.Init();

            var snapshot = await _snapshotRepository.ReadSnapshotAsync(AggregateId);
            LastEventNumberInSnapshot = snapshot.Item1;
            if (LastEventNumberInSnapshot != 0)
                _writeModel = await ModelSerializationExt.Deserialize(_logFile, snapshot.Item2);

            var eventStream = _storeEvents.OpenStream(AggregateId);

            if (LastEventNumberInSnapshot == 0 && eventStream.CommittedEvents.FirstOrDefault() == null)
            {
                Seed().Wait();
                _logFile.AppendLine("Empty graph is seeded with default zone and users.");
            }

            var events = eventStream.CommittedEvents.Select(x => x.Body).Skip(LastEventNumberInSnapshot).ToList();
            _logFile.AppendLine($"{events.Count} events should be applied...");
            foreach (var evnt in events)
            {
                _writeModel.Apply(evnt);
            }
            _logFile.AppendLine("Events applied successfully.");

            return events.Count;
        }

        public void Delete()
        {
            _eventStoreInitializer.DropDatabase();
        }


        private async Task<string> Seed()
        {
            var cmd = new ApplyLicense()
            {
                Owner = "Demo license",
                RtuCount = new LicenseParameter() { Value = 1, ValidUntil = DateTime.MaxValue },
                ClientStationCount = new LicenseParameter() { Value = 2, ValidUntil = DateTime.MaxValue },
                SuperClientStationCount = new LicenseParameter() { Value = 1, ValidUntil = DateTime.Today.AddMonths(6) },
                Version = "2.0.0.0"
            };
            await SendCommand(cmd, "developer", "OnServer");
            await SendCommand(new AddZone() { IsDefaultZone = true, Title = StringResources.Resources.SID_Default_Zone }, "developer", "OnServer");

            await SendCommand(new AddUser() { UserId = Guid.NewGuid(), Title = "developer", EncodedPassword = UserExt.FlipFlop("developer"), Role = Role.Developer, ZoneId = Guid.Empty }, "developer", "OnServer");
            await SendCommand(new AddUser() { UserId = Guid.NewGuid(), Title = "root", EncodedPassword = UserExt.FlipFlop("root"), Role = Role.Root, ZoneId = Guid.Empty }, "developer", "OnServer");
            await SendCommand(new AddUser() { UserId = Guid.NewGuid(), Title = "operator", EncodedPassword = UserExt.FlipFlop("operator"), Role = Role.Operator, ZoneId = Guid.Empty }, "developer", "OnServer");
            await SendCommand(new AddUser() { UserId = Guid.NewGuid(), Title = "supervisor", EncodedPassword = UserExt.FlipFlop("supervisor"), Role = Role.Supervisor, ZoneId = Guid.Empty }, "developer", "OnServer");
            await SendCommand(new AddUser() { UserId = Guid.NewGuid(), Title = "superclient", EncodedPassword = UserExt.FlipFlop("superclient"), Role = Role.Superclient, ZoneId = Guid.Empty }, "developer", "OnServer");
            return null;
        }

        // especially for Migrator.exe
        public Task<int> SendCommands(List<object> cmds, string username, string clientIp)
        {
            foreach (var cmd in cmds)
            {
                var result = _commandAggregator.Validate(cmd);
                if (!string.IsNullOrEmpty(result))
                    _logFile.AppendLine(result);
            }

            StoreEventsInDb(username, clientIp);
            return Task.FromResult(cmds.Count);
        }

        public Task<string> SendCommand(object cmd, string username, string clientIp)
        {
            // ilya: can pass user id\role as an argument to When to check permissions
            var result = _commandAggregator.Validate(cmd); // Aggregate checks if command is valid
                                                           // and if so, transforms command into event and passes it to WriteModel
                                                           // WriteModel applies event and returns whether event was applied successfully

            if (result == null)                                   // if command was valid and applied successfully it should be persisted
                StoreEventsInDb(username, clientIp);
            return Task.FromResult(result);
        }

        private void StoreEventsInDb(string username, string clientIp)
        {
            var eventStream = _storeEvents.OpenStream(AggregateId);
            foreach (var e in _eventsQueue.EventsWaitingForCommit)   // takes already applied event(s) from WriteModel's list
            {
                eventStream.Add(WrapEvent(e, username, clientIp));   // and stores this event in BD
            }
            _eventsQueue.Commit();                                     // now cleans WriteModel's list
            eventStream.CommitChanges(Guid.NewGuid());
        }

        private EventMessage WrapEvent(object e, string username, string clientIp)
        {
            var msg = new EventMessage();
            msg.Headers.Add("Timestamp", DateTime.Now);
            msg.Headers.Add("Username", username);
            msg.Headers.Add("ClientIp", clientIp);
            msg.Headers.Add("VersionId", AggregateId);
            msg.Body = e;
            return msg;
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

        public int RemoveEvents(int fromRevision, int upToRevision)
        {
            _logFile.AppendLine("Removing events included into snapshot");
            var count = upToRevision - fromRevision;
            var eventStream = _storeEvents.OpenStream(AggregateId);
            var listForRemove = eventStream.CommittedEvents.Select(x => x).Take(count).ToArray();
            var cc = 0;
            foreach (var evnt in listForRemove)
            {
                eventStream.CommittedEvents.Remove(evnt);
                cc++;
                if (cc % 100 == 0)
                    _logFile.AppendLine($"{cc} events removed");
            }
            return count;
        }
    }
}