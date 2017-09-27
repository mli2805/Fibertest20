using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;
using NEventStore;
using PrivateReflectionUsingDynamic;
using WcfConnections;
using WcfServiceForClientLibrary;

namespace DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WcfServiceForClient : IWcfServiceForClient
    {
        // BUG: Initialize this!
        private readonly EventStoreService _service = new EventStoreService();

        public static IniFile ServiceIniFile { get; set; }
        public static IMyLog ServiceLog { get; set; }


        public static event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);

        public ConcurrentDictionary<Guid, ClientStation> ClientComps;

        public string SendCommand(string json) => _service.SendCommand(json);
        public string[] GetEvents(int revision) => _service.GetEvents(revision);

        public WcfServiceForClient(ConcurrentDictionary<Guid, ClientStation> clientComps)
        {
            ClientComps = clientComps;
        }

        public Task<ClientRegisteredDto> MakeExperimentAsync(RegisterClientDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} makes an experiment");
            var result = new ClientRegisteredDto();


            if (ClientComps.ContainsKey(dto.ClientId))
            {
                ClientStation oldClient;
                ClientComps.TryRemove(dto.ClientId, out oldClient);
            }

            var client = new ClientStation()
            {
                Id = dto.ClientId,
                PcAddresses = new DoubleAddressWithLastConnectionCheck() { DoubleAddress = dto.Addresses }
            };
            result.IsRegistered = ClientComps.TryAdd(dto.ClientId, client);

            ServiceLog.AppendLine($"There are {ClientComps.Count} clients");
            return Task.FromResult(result);
        }

        public void RegisterClient(RegisterClientDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent register request");
            MessageReceived?.Invoke(dto);
        }

        public void UnRegisterClient(UnRegisterClientDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent unregister request");
            MessageReceived?.Invoke(dto);
        }

        public bool CheckServerConnection(CheckServerConnectionDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} checked server connection");
            return true;
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent check rtu {dto.RtuId.First6()} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public Task<RtuInitializedDto> InitializeRtuLongTask(InitializeRtuDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent initialize rtu {dto.RtuId.First6()} request");
            var d2RWcfManager = new D2RWcfManager(dto.RtuAddresses, ServiceIniFile, ServiceLog);
            return d2RWcfManager.InitializeRtuLongTask(dto);
        }

        public bool InitializeRtu(InitializeRtuDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent initialize rtu {dto.RtuId.First6()} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool StartMonitoring(StartMonitoringDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent start monitoring on rtu {dto.RtuId.First6()} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent stop monitoring on rtu {dto.RtuId.First6()} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent monitoring settings for rtu {dto.RtuId.First6()}");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool AssignBaseRef(AssignBaseRefDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent base ref for trace on rtu {dto.RtuId.First6()}");
            MessageReceived?.Invoke(dto);
            return true;
        }
    }
    //TODO: Either merge projects, or use an interface
    public class EventStoreService
    {
        private IStoreEvents _storeEvents;
        private readonly Aggregate _aggregate;
        private static readonly Guid AggregateId =
            new Guid("1C28CBB5-A9F5-4A5C-B7AF-3D188F8F24ED");

        private WriteModel _writeModel;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public EventStoreService()
        {
            _storeEvents = Wireup.Init()
                // .UsingSqlPersistence("myDbConnectionStringName")
                .UsingInMemoryPersistence()
                .InitializeStorageEngine()
                // .WithPersistence()
                .Build();

            var eventStream = _storeEvents.OpenStream(AggregateId);
            var events = eventStream.CommittedEvents.Select(x => x.Body);

            _writeModel = new WriteModel(events);
            _aggregate = new Aggregate(_writeModel);
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
