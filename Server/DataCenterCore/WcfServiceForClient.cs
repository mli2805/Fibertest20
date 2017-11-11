using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForClient : IWcfServiceForClient
    {
        private readonly EventStoreService _eventStoreService;

        private readonly IMyLog _logFile;

        private readonly ClientRegistrationManager _clientRegistrationManager;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly RtuRegistrationManager _rtuRegistrationManager;
        private readonly BaseRefManager _baseRefManager;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public WcfServiceForClient(IMyLog logFile, EventStoreService eventStoreService, 
            ClientRegistrationManager clientRegistrationManager, ClientToRtuTransmitter clientToRtuTransmitter,
            RtuRegistrationManager rtuRegistrationManager, BaseRefManager baseRefManager)
        {
            _logFile = logFile;
            _eventStoreService = eventStoreService;
            _clientRegistrationManager = clientRegistrationManager;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _rtuRegistrationManager = rtuRegistrationManager;
            _baseRefManager = baseRefManager;
        }

        public async Task<string> SendCommandAsObj(object cmd)
        {
            return await _eventStoreService.SendCommand(cmd);
        }

        public async Task<string> SendCommand(string json)
        {
            var cmd = JsonConvert.DeserializeObject(json, JsonSerializerSettings);

            var resultInGraph = await _eventStoreService.SendCommand(cmd);
            if (resultInGraph != null)
                return resultInGraph;

           // A few commands need postprocessing in Db or RTU
            var removeRtu = cmd as RemoveRtu;
            if (removeRtu != null)
                return await _rtuRegistrationManager.RemoveRtuAsync(removeRtu.Id);

            // var attachTrace = cmd as AttachTrace;
            // if trace has base refs they should be sent to RTU
            // if (attachTrace != null)
            //    
            return null;
        }

        public async Task<string[]> GetEvents(int revision)
        {
            return await Task.FromResult(_eventStoreService.GetEvents(revision));
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            return await _clientRegistrationManager.RegisterClientAsync(dto);
        }

        public async Task UnregisterClientAsync(UnRegisterClientDto dto)
        {
            await _clientRegistrationManager.UnregisterClientAsync(dto);
            _logFile.AppendLine($"Client {dto.ClientId.First6()} exited");
        }

        public Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} checked server connection");
            return Task.FromResult(true);
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} check rtu {dto.NetAddress.ToStringA()} connection");
            return await _clientToRtuTransmitter.CheckRtuConnectionAsync(dto);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent initialize rtu {dto.RtuId.First6()} request");
            var result = await _clientToRtuTransmitter.InitializeAsync(dto);
            _logFile.AppendLine($"Initialization result is {result.IsInitialized}");
            return result;
        }


        public async Task<bool> StartMonitoringAsync(StartMonitoringDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent start monitoring on rtu {dto.RtuId.First6()} request");
            var result = await _clientToRtuTransmitter.StartMonitoringAsync(dto);
            _logFile.AppendLine($"Start monitoring result is {result}");
            return result;
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent stop monitoring on rtu {dto.RtuId.First6()} request");
            var result = await _clientToRtuTransmitter.StopMonitoringAsync(dto);
            _logFile.AppendLine($"Stop monitoring result is {result}");
            return result;
        }

        public async Task<bool> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent monitoring settings for rtu {dto.RtuId.First6()}");
            var result = await _clientToRtuTransmitter.ApplyMonitoringSettingsAsync(dto);
            _logFile.AppendLine($"Apply monitoring settings result is {result}");
            return result;
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent base ref for trace {dto.TraceId.First6()}");
            var result = await _baseRefManager.AddUpdateOrRemoveBaseRef(dto);
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                return result;

            if (dto.OtauPortDto == null) // unattached trace
                return result;

            return await _clientToRtuTransmitter.AssignBaseRefAsync(dto);
        }
    }
}
