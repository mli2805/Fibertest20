using System.ServiceModel;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfServiceForRtuInterface
{
    [ServiceContract]
    public interface IWcfServiceForRtu
    {

        // RTU notifies

        [OperationContract]
        bool KnowRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto monitoringStep);

        [OperationContract]
        void RegisterRtuHeartbeat(RtuChecksChannelDto result);


        [OperationContract]
        bool ProcessMonitoringResult(MonitoringResultDto result);

    }
}
