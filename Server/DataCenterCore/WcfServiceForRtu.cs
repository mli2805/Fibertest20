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
        private readonly DcManager _dcManager;
        private readonly IniFile _iniFile;
        private readonly IMyLog _serviceLog;

        public static event OnMessageReceived MessageReceived;
        public delegate bool OnMessageReceived(object e);

        public ConcurrentDictionary<Guid, RtuStation> RtuStations;
        public ConcurrentDictionary<Guid, ClientStation> ClientComps;

        public WcfServiceForRtu(DcManager dcManager, IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _serviceLog = logFile;
            _dcManager = dcManager;
        }


        // RTU responses on DataCenter's (Client's) requestes
        public bool ProcessRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            _serviceLog.AppendLine($"Rtu {dto.RtuId} reply on connection check request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ProcessRtuInitialized(RtuInitializedDto dto)
        {
            _serviceLog.AppendLine($"Rtu {dto.Serial} reply on initialize request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmStartMonitoring(MonitoringStartedDto dto)
        {
            _serviceLog.AppendLine($"Rtu {dto.RtuId.First6()} reply on start monitoring request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmStopMonitoring(MonitoringStoppedDto dto)
        {
            _serviceLog.AppendLine($"Rtu {dto.RtuId.First6()} reply on stop monitoring request");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto dto)
        {
            _serviceLog.AppendLine($"Rtu {dto.RtuId.First6()} reply on monitoring settings");
            MessageReceived?.Invoke(dto);
            return true;
        }

        public bool ConfirmBaseRefAssigned(BaseRefAssignedDto dto)
        {
            _serviceLog.AppendLine($"Rtu {dto.RtuId.First6()} reply on base ref");
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
