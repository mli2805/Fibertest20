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
        const string Timestamp = @"Timestamp";
        private readonly IMyLog _logFile;
        private readonly IEventStoreInitializer _eventStoreInitializer;
        private readonly SnapshotRepository _snapshotRepository;
        public IStoreEvents StoreEvents;
        private readonly CommandAggregator _commandAggregator;
        private readonly EventsQueue _eventsQueue;
        private readonly Model _writeModel;

        public Guid StreamIdOriginal;

        private readonly int _eventsPortion;
        public int LastEventNumberInSnapshot;
        public DateTime LastEventDateInSnapshot;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public EventStoreService(IniFile iniFile, IMyLog logFile, IEventStoreInitializer eventStoreInitializer,
            SnapshotRepository snapshotRepository,
             CommandAggregator commandAggregator, EventsQueue eventsQueue, Model writeModel)
        {
            _eventsPortion = iniFile.Read(IniSection.General, IniKey.EventSourcingPortion, 100);
            _logFile = logFile;
            _eventStoreInitializer = eventStoreInitializer;
            _snapshotRepository = snapshotRepository;
            _commandAggregator = commandAggregator;
            _eventsQueue = eventsQueue;
            _writeModel = writeModel;
        }

        public async Task<int> Init()
        {
            StoreEvents = _eventStoreInitializer.Init();

            var snapshot = await _snapshotRepository.ReadSnapshotAsync(StreamIdOriginal);
            LastEventNumberInSnapshot = snapshot.Item1;
            LastEventDateInSnapshot = snapshot.Item3;
            if (LastEventNumberInSnapshot != 0)
            {
                if (!await _writeModel.Deserialize(_logFile, snapshot.Item2)) return -1;
            }

            var eventStream = StoreEvents.OpenStream(StreamIdOriginal);

            if (LastEventNumberInSnapshot == 0 && eventStream.CommittedEvents.FirstOrDefault() == null)
            {
                foreach (object seed in DbSeeds.Collection)
                    await SendCommand(seed, "developer", "OnServer");

                _logFile.AppendLine("Empty graph is seeded with default zone and users.");
            }

            var events = eventStream.CommittedEvents.Select(x => x.Body).ToList();
            _logFile.AppendLine($"{events.Count} events should be applied...");
            foreach (var evnt in events)
            {
                _writeModel.Apply(evnt);
            }
            _logFile.AppendLine("Events applied successfully.");
            _logFile.AppendLine($"Last event number is {LastEventNumberInSnapshot + events.Count}");

            // await SeedOlts();

            var msg = eventStream.CommittedEvents.LastOrDefault();
            if (msg != null)
                _logFile.AppendLine($@"Last applied event has timestamp {msg.Headers[Timestamp]:O}");

            return events.Count;
        }

        private async Task SeedOlts()
        {
            _writeModel.Olts.Clear();

            var rtu53 = _writeModel.Rtus.FirstOrDefault(r => r.Title == "53");
            if (rtu53 == null) return;

            var otauDto = rtu53.Children.First().Value;
            var otau = _writeModel.Otaus.First(o => o.Serial == otauDto.Serial);
            var otauPort = new OtauPortDto()
            {
                OtauId = otau.Id.ToString(),
                Serial = otauDto.Serial,
                IsPortOnMainCharon = false,
                OpticalPort = 1,
            };
            var olt21Id = Guid.NewGuid();
            var olt9714Id = Guid.NewGuid();
            var olt59Id = Guid.NewGuid();

            var seeds = new List<object>()
            {
                new AddOlt()
                {
                    Id = olt21Id,
                    Ip = @"192.168.96.21",
                    OltModel = OltModel.Huawei_MA5608T,
                },

                new AddOlt()
                {
                    Id = olt9714Id,
                    Ip = @"192.168.97.14",
                    OltModel = OltModel.Huawei_MA5608T,
                },

                new AddOlt()
                {
                    Id = olt59Id,
                    Ip = @"192.168.96.59",
                    OltModel = OltModel.Huawei_MA5608T,
                },


                new AddGponPortRelation()
                {
                    Id = Guid.NewGuid(),
                    OltId = olt21Id,
                    GponInterface = 5,
                    RtuId = rtu53.Id,
                    OtauPort = otauPort,
                },

                new AddGponPortRelation()
                {
                    Id = Guid.NewGuid(),
                    OltId = olt9714Id,
                    GponInterface = 5,
                    RtuId = rtu53.Id,
                    OtauPort = otauPort,
                },

                new AddGponPortRelation()
                {
                    Id = Guid.NewGuid(),
                    OltId = olt59Id,
                    GponInterface = 5,
                    RtuId = rtu53.Id,
                    OtauPort = otauPort,
                },
            };

            foreach (object seed in seeds)
            {
                var ss = await SendCommand(seed, "developer", "OnServer");
                if (ss != null)
                    _logFile.AppendLine(ss);
            }
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
            var eventStream = StoreEvents.OpenStream(StreamIdOriginal);
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
            msg.Headers.Add("VersionId", StreamIdOriginal);
            msg.Body = e;
            return msg;
        }

        public string[] GetEvents(int revision)
        {
            try
            {
                var events = StoreEvents
                    .OpenStream(StreamIdOriginal, revision + 1)
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

        public bool CompareEvent(int revision, DateTime timestamp)
        {
            try
            {
                var evnt = StoreEvents.OpenStream(StreamIdOriginal, revision + 1).CommittedEvents.FirstOrDefault();
                if (evnt == null)
                {
                    _logFile.AppendLine($"CompareLastEvent: there is no event with revision {revision}");
                    return false;
                }

                if ((DateTime)evnt.Headers[Timestamp] == timestamp)
                {
                    _logFile.AppendLine($"CompareLastEvent: Events {revision} are the same on server and client");
                    return true;
                }
                else
                {
                    _logFile.AppendLine($"CompareLastEvent: Events {revision} are DIFFERENT on server and client");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"CompareLastEvent: {e.Message}");
                return false;
            }
        }

    }
}