using System.ServiceModel;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfServiceForRtuInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        private readonly DcManager _dcManager;

        public WcfServiceForRtu(DcManager dcManager)
        {
            _dcManager = dcManager;
        }

        #region RTU responses on DataCenter's (Client's) requestes
        public bool ProcessRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            return _dcManager.ProcessRtuConnectionChecked(dto);
        }

       public bool ConfirmStartMonitoring(MonitoringStartedDto dto)
        {
            return _dcManager.ConfirmMonitoringStarted(dto);
        }

        public bool ConfirmStopMonitoring(MonitoringStoppedDto dto)
        {
            return _dcManager.ConfirmMonitoringStopped(dto);
        }

        public bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto dto)
        {
            return _dcManager.ConfirmMonitoringSettingsApplied(dto);
        }

        public bool ConfirmBaseRefAssigned(BaseRefAssignedDto dto)
        {
            return _dcManager.ConfirmBaseRefAssigned(dto);
        }
        #endregion

        #region RTU notifies
        public bool KnowRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
            return _dcManager.ProcessRtuCurrentMonitoringStep(dto);
        }

        public bool ProcessRtuChecksChannel(RtuChecksChannelDto dto)
        {
            return _dcManager.ProcessRtuChecksChannel(dto);
        }

        public bool ProcessMonitoringResult(MonitoringResultDto dto)
        {
            return _dcManager.ProcessMonitoringResult(dto);
        }
        #endregion
    }
}
