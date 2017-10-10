using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WcfServiceForClient : IWcfServiceForClient
    {
        // BUG: Initialize this!
        private readonly EventStoreService _eventStoreService = new EventStoreService();

        private readonly IMyLog _logFile;

        private readonly DcManager _dcManager;

        public WcfServiceForClient(DcManager dcManager, IMyLog logFile)
        {
            _logFile = logFile;
            _dcManager = dcManager;
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

        public bool CheckServerConnection(CheckServerConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} checked server connection");
            return true;
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} check rtu {dto.RtuId.First6()} connection");
            return await _dcManager.CheckRtuConnectionAsync(dto);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            //            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent initialize rtu {dto.RtuId.First6()} request");
            //            RtuInitializedDto b = new RtuInitializedDto();
            //            try
            //            {
            //                b = await _dcManager.InitializeRtuAsync(dto);
            //            }
            //            catch (Exception e)
            //            {
            //                _logFile.AppendLine($"{e.Message}");
            //            }
            //            _logFile.AppendLine($"Initialization terminated. Result is {b.IsInitialized}");
            //            return b;

//                        return await _dcManager.InitializeRtuAsync(dto);

            _dcManager.InitializeThroughBeginEnd(dto);
            return new RtuInitializedDto();
        }


        public bool StartMonitoring(StartMonitoringDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent start monitoring on rtu {dto.RtuId.First6()} request");
            _dcManager.HandleMessage(dto);
            return true;
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent stop monitoring on rtu {dto.RtuId.First6()} request");
            _dcManager.HandleMessage(dto);
            return true;
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent monitoring settings for rtu {dto.RtuId.First6()}");
            _dcManager.HandleMessage(dto);
            return true;
        }

        public bool AssignBaseRef(AssignBaseRefDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent base ref for trace on rtu {dto.RtuId.First6()}");
            _dcManager.HandleMessage(dto);
            return true;
        }
    }
}
