using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForRtuInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        public static IMyLog ServiceLog { get; set; }

        public static event OnMessageReceived MessageReceived;
        public delegate bool OnMessageReceived(object e);

        public ConcurrentDictionary<Guid, RtuStation> RtuStations;
        public ConcurrentDictionary<Guid, ClientStation> ClientComps;

//        public WcfServiceForRtu()
//        {
//        }

        public WcfServiceForRtu(ConcurrentDictionary<Guid, RtuStation> rtuStations, ConcurrentDictionary<Guid, ClientStation> clientComps)
        {
            RtuStations = rtuStations;
            ClientComps = clientComps;
        }

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
            //            LogFile.AppendLine($"Transfer Rtu's {dto.RtuId.First6()} current monitoring step");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ProcessRtuChecksChannel(RtuChecksChannelDto dto)
        {
            //            var channel = dto.IsMainChannel ? "MAIN" : "RESERVE";
            //            LogFile.AppendLine($"Rtu {dto.RtuId.First6()} checks {channel} channel");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ProcessMonitoringResult(MonitoringResultDto dto)
        {
//            LogFile.AppendLine($"Rtu {dto.RtuId.First6()} sent monitoring result");
            MessageReceived?.Invoke(dto);
            return true;
        }

    }
}
