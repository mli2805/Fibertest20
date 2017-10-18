using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForClient : IWcfServiceForClient
    {
        // BUG: Initialize this!
        private readonly EventStoreService _eventStoreService;

        private readonly IMyLog _logFile;

        private readonly DcManager _dcManager;

        public WcfServiceForClient(EventStoreService eventStoreService, DcManager dcManager, IMyLog logFile)
        {
            _logFile = logFile;
            _dcManager = dcManager;
            _eventStoreService = eventStoreService;
        }

        public async Task<string> SendCommandAsObj(object cmd)
        {
            return await Task.FromResult(_eventStoreService.SendCommand(cmd));
        }

        public async Task<string> SendCommand(string json)
        {
            return await Task.FromResult(_eventStoreService.SendCommand(json));
        }

        public async Task<string[]> GetEvents(int revision)
        {
            return await Task.FromResult(_eventStoreService.GetEvents(revision));
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} asks registration");
            return await _dcManager.RegisterClientAsync(dto);
        }

        public async Task UnregisterClientAsync(UnRegisterClientDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} exited");
            await _dcManager.UnregisterClientAsync(dto);
        }

        public Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} checked server connection");
            return Task.FromResult(true);
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} check rtu {dto.NetAddress.ToStringA()} connection");
            return await _dcManager.CheckRtuConnectionAsync(dto);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent initialize rtu {dto.RtuId.First6()} request");
            var result = await _dcManager.InitializeAsync(dto);
            _logFile.AppendLine($"Initialization result is {result.IsInitialized}");
            return result;
        }


        public async Task<bool> StartMonitoringAsync(StartMonitoringDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent start monitoring on rtu {dto.RtuId.First6()} request");
            var result = await _dcManager.StartMonitoringAsync(dto);
            _logFile.AppendLine($"Start monitoring result is {result}");
            return result;
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent stop monitoring on rtu {dto.RtuId.First6()} request");
            var result = await _dcManager.StopMonitoringAsync(dto);
            _logFile.AppendLine($"Stop monitoring result is {result}");
            return result;
        }

        public async Task<bool> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent monitoring settings for rtu {dto.RtuId.First6()}");
            var result = await _dcManager.ApplyMonitoringSettingsAsync(dto);
            _logFile.AppendLine($"Apply monitoring settings result is {result}");
            return result;
        }

        public async Task<bool> AssignBaseRefAsync(AssignBaseRefDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent base ref for trace on rtu {dto.RtuId.First6()}");
            var result = await _dcManager.AssignBaseRefAsync(dto);
            _logFile.AppendLine($"Assign base ref result is {result}");
            return result;
        }
    }
}
