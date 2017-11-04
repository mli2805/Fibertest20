using System.ServiceModel;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfServiceForRtuInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        private readonly MonitoringResultsManager _monitoringResultsManager;
        private readonly RtuRegistrationManager _rtuRegistrationManager;

        public WcfServiceForRtu(MonitoringResultsManager monitoringResultsManager, RtuRegistrationManager rtuRegistrationManager)
        {
            _monitoringResultsManager = monitoringResultsManager;
            _rtuRegistrationManager = rtuRegistrationManager;
        }

       #region RTU notifies
        public bool KnowRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
            return _monitoringResultsManager.ProcessRtuCurrentMonitoringStep(dto);
        }

        public void RegisterRtuHeartbeat(RtuChecksChannelDto dto)
        {
            _rtuRegistrationManager.RegisterRtuHeartbeatAsync(dto).Wait();
        }
        #endregion
    }
}
