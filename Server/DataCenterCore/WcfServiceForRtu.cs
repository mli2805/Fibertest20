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
