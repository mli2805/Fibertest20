using Dto;
using Iit.Fibertest.Utils35;

namespace WcfServiceForRtuLibrary
{
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        public static Logger35 ServiceLog { get; set; }

        public static event OnMessageReceived MessageReceived;
        public delegate bool OnMessageReceived(object e);

        public bool ProcessRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            ServiceLog.AppendLine($"Rtu {dto.RtuId} reply on connection check request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ProcessRtuInitialized(RtuInitializedDto dto)
        {
            ServiceLog.AppendLine($"Rtu {dto.Serial} reply on initialize request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmStartMonitoring(MonitoringStartedDto dto)
        {
            ServiceLog.AppendLine($"Rtu {dto.RtuId} reply on start monitoring request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmStopMonitoring(MonitoringStoppedDto dto)
        {
            ServiceLog.AppendLine($"Rtu {dto.RtuId} reply on stop monitoring request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ProcessMonitoringResult(SaveMonitoringResultDto dto)
        {
            ServiceLog.AppendLine($"Rtu {dto.RtuId} sent monitoring result");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto dto)
        {
            ServiceLog.AppendLine($"Rtu {dto.RtuId} reply on monitoring settings");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmBaseRefAssigned(BaseRefAssignedDto dto)
        {
            ServiceLog.AppendLine($"Rtu {dto.RtuId} reply on base ref");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool KnowRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
//            ServiceLog.AppendLine($"Transfer Rtu's {dto.RtuId} current monitoring step");
            MessageReceived?.Invoke(dto);
            return true;
        }
    }
}
