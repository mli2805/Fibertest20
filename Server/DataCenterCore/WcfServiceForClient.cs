using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WcfServiceForClient : IWcfServiceForClient
    {
        // BUG: Initialize this!
        private readonly EventStoreService _service = new EventStoreService();

        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private readonly DcManager _dcManager; 

        public string SendCommand(string json) => _service.SendCommand(json);
        public string[] GetEvents(int revision) => _service.GetEvents(revision);
        

        public WcfServiceForClient(DcManager dcManager, IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _dcManager = dcManager;
        }

        public Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            return Task.FromResult(_dcManager.RegisterClient(dto));
        }

        public void UnRegisterClient(UnRegisterClientDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent unregister request");
            _dcManager.HandleMessage(dto);
        }

        public bool CheckServerConnection(CheckServerConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} checked server connection");
            return true;
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent check rtu {dto.RtuId.First6()} request");
            _dcManager.HandleMessage(dto);
            return true;
        }



        private D2RWcfManager _d2RWcfManager;
        public void InitializeRtuLongTask(InitializeRtuDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent initialize rtu {dto.RtuId.First6()} request");
            _d2RWcfManager = new D2RWcfManager(dto.RtuAddresses, _iniFile, _logFile);

            _d2RWcfManager.InitializeRtuLongTask(dto, InitializeRtuLongTaskCallback);
        }

        private void InitializeRtuLongTaskCallback(IAsyncResult asyncState)
        {
            try
            {
                if (_d2RWcfManager == null)
                    return;

                var result = _d2RWcfManager.InitializeRtuLongTaskEnd(asyncState);
                _logFile.AppendLine($@"{result.Version}");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }








        public bool InitializeRtu(InitializeRtuDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent initialize rtu {dto.RtuId.First6()} request");
            _dcManager.HandleMessage(dto);
            return true;
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
