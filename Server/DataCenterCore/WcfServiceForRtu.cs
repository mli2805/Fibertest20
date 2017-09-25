using Dto;
using Iit.Fibertest.UtilsLib;
using WcfServiceForRtuLibrary;

namespace DataCenterCore
{
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        public static IMyLog ServiceLog { get; set; }

        public static event OnMessageReceived MessageReceived;
        public delegate bool OnMessageReceived(object e);

        // RTU responses on DataCenter's (Client's) requestes

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
            ServiceLog.AppendLine($"Rtu {dto.RtuId.First6()} reply on start monitoring request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmStopMonitoring(MonitoringStoppedDto dto)
        {
            ServiceLog.AppendLine($"Rtu {dto.RtuId.First6()} reply on stop monitoring request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto dto)
        {
            ServiceLog.AppendLine($"Rtu {dto.RtuId.First6()} reply on monitoring settings");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmBaseRefAssigned(BaseRefAssignedDto dto)
        {
            ServiceLog.AppendLine($"Rtu {dto.RtuId.First6()} reply on base ref");
            MessageReceived?.Invoke(dto);
            return true;
        }

        // RTU notifies

        public bool KnowRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
            //            ServiceLog.AppendLine($"Transfer Rtu's {dto.RtuId.First6()} current monitoring step");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ProcessRtuChecksChannel(RtuChecksChannelDto dto)
        {
            //            var channel = dto.IsMainChannel ? "MAIN" : "RESERVE";
            //            ServiceLog.AppendLine($"Rtu {dto.RtuId.First6()} checks {channel} channel");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ProcessMonitoringResult(MonitoringResultDto dto)
        {
//            ServiceLog.AppendLine($"Rtu {dto.RtuId.First6()} sent monitoring result");
            MessageReceived?.Invoke(dto);
            return true;
        }

    }
}
