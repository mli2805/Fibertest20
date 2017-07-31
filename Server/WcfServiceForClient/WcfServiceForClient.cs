using Dto;
using Iit.Fibertest.Utils35;

namespace WcfServiceForClientLibrary
{
    public class WcfServiceForClient : IWcfServiceForClient
    {
        public static Logger35 ServiceLog { get; set; }

        public static event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);


        public void RegisterClient(RegisterClientDto dto)
        {
            ServiceLog.AppendLine($"Client from {dto.ClientAddress} sent register request");
            MessageReceived?.Invoke(dto);
        }

        public void UnRegisterClient(UnRegisterClientDto dto)
        {
            ServiceLog.AppendLine($"Client from {dto.ClientAddress} sent unregister request");
            MessageReceived?.Invoke(dto);
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            ServiceLog.AppendLine($"Client from {dto.ClientAddress} sent check rtu {dto.RtuId} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool InitializeRtu(InitializeRtuDto dto)
        {
            ServiceLog.AppendLine($"Client from {dto.ClientAddress} sent initialize rtu {dto.RtuId} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool StartMonitoring(StartMonitoringDto dto)
        {
            ServiceLog.AppendLine($"Client from {dto.ClientAddress} sent start monitoring on rtu {dto.RtuAddress} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            ServiceLog.AppendLine($"Client from {dto.ClientAddress} sent stop monitoring on rtu {dto.RtuAddress} request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            ServiceLog.AppendLine($"Client from {dto.ClientAddress} sent monitoring settings for rtu {dto.RtuIpAddress}");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool AssignBaseRef(AssignBaseRefDto dto)
        {
            ServiceLog.AppendLine($"Client from {dto.ClientAddress} sent base ref for trace on rtu {dto.RtuIpAddress}");
            MessageReceived?.Invoke(dto);
            return true;
        }
    }
}
