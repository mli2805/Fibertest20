using System.ServiceModel;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfServiceForRtuInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        private readonly DcManager _dcManager;
        private readonly RtuRegistrationManager _rtuRegistrationManager;

        public WcfServiceForRtu(DcManager dcManager, RtuRegistrationManager rtuRegistrationManager)
        {
            _dcManager = dcManager;
            _rtuRegistrationManager = rtuRegistrationManager;
        }

       #region RTU notifies
        public bool KnowRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
            return _dcManager.ProcessRtuCurrentMonitoringStep(dto);
        }

        public void RegisterRtuHeartbeat(RtuChecksChannelDto dto)
        {
            _rtuRegistrationManager.RegisterRtuHeartbeatAsync(dto).Wait();
        }

        public bool ProcessMonitoringResult(MonitoringResultDto dto)
        {
            return _dcManager.ProcessMonitoringResult(dto);
        }
        #endregion
    }
}
