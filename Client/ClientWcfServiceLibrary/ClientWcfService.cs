using System.ServiceModel;
using Dto;
using Iit.Fibertest.Utils35;

namespace ClientWcfServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        public static LogFile ClientLog { get; set; }

        public static event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);


        public void ConfirmDelivery(RtuCommandDeliveredDto dto)
        {
            if (dto.MessageProcessingResult == MessageProcessingResult.UnknownRtu)
                ClientLog.AppendLine($"Server has no record about RTU {dto.RtuId}");
            if (dto.MessageProcessingResult == MessageProcessingResult.FailedToTransmit)
                ClientLog.AppendLine($"Cannot deliver command to RTU {dto.RtuId}");
            if (dto.MessageProcessingResult == MessageProcessingResult.TransmittedSuccessfullyButRtuIsBusy)
                ClientLog.AppendLine($"Command was delivered to RTU {dto.RtuId} but RTU ignored it (RTU is busy)");

            MessageReceived?.Invoke(dto);
        }

        public void ConfirmRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            ClientLog.AppendLine($"RTU {dto.RtuId} connection check received.");
            MessageReceived?.Invoke(dto);
        }

        public void ConfirmRtuInitialized(RtuInitializedDto rtu)
        {
            ClientLog.AppendLine($"RTU serial={rtu.Serial} confirmed initialization");
            MessageReceived?.Invoke(rtu);
        }

        public void ConfirmMonitoringStarted(MonitoringStartedDto confirm)
        {
            var result = confirm.IsSuccessful ? "confirmed: monitoring started." : "ERROR, can't start monitoring ";
            ClientLog.AppendLine($"Rtu {confirm.RtuId} {result}");
            MessageReceived?.Invoke(confirm);
        }
        public void ConfirmMonitoringStopped(MonitoringStoppedDto confirm)
        {
            var result = confirm.IsSuccessful ? "confirmed: monitoring stopped." : "ERROR, can't stop monitoring ";
            ClientLog.AppendLine($"Rtu {confirm.RtuId} {result}");
            MessageReceived?.Invoke(confirm);
        }

        public void ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto confirm)
        {
            ClientLog.AppendLine($"RTU {confirm.RtuId} monitoring settings applied: {confirm.IsSuccessful}");
            MessageReceived?.Invoke(confirm);
        }

        public void ConfirmBaseRefAssigned(BaseRefAssignedDto confirm)
        {
            ClientLog.AppendLine($"RTU {confirm.RtuId} base ref assigned: {confirm.IsSuccessful}");
            MessageReceived?.Invoke(confirm);
        }

        public void ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
//            ClientLog.AppendLine($"RTU {dto.RtuId} sent current state: {dto.MonitoringStep}");
            MessageReceived?.Invoke(dto);
        }
    }
}
