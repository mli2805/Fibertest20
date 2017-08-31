using Dto;
using Iit.Fibertest.UtilsLib;

namespace WcfServiceForClientLibrary
{
    public class WcfServiceForClient : IWcfServiceForClient
    {
        public static LogFile ServiceLog { get; set; }

        public static event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);


        public void RegisterClient(RegisterClientDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId} sent register request");
            MessageReceived?.Invoke(dto);
        }

        public void UnRegisterClient(UnRegisterClientDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId} sent unregister request");
            MessageReceived?.Invoke(dto);
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId} sent check rtu {dto.RtuId} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool InitializeRtu(InitializeRtuDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId} sent initialize rtu {dto.RtuId} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool StartMonitoring(StartMonitoringDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId} sent start monitoring on rtu {dto.RtuId} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId} sent stop monitoring on rtu {dto.RtuId} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId} sent monitoring settings for rtu {dto.RtuId}");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool AssignBaseRef(AssignBaseRefDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId} sent base ref for trace on rtu {dto.RtuId}");
            MessageReceived?.Invoke(dto);
            return true;
        }
    }
}
