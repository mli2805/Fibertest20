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
        bool ProcessRtuChecksChannel(RtuChecksChannelDto result);

        [OperationContract]
        bool ProcessMonitoringResult(MonitoringResultDto result);

    }
}
