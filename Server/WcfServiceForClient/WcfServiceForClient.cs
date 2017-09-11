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
}
