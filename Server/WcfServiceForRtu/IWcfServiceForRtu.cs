using System.ServiceModel;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfServiceForRtuInterface
{
    [ServiceContract]
    public interface IWcfServiceForRtu
    {
        // RTU responses on DataCenter's (Client's) requestes

        [OperationContract]
        bool ProcessRtuConnectionChecked(RtuConnectionCheckedDto result);

        [OperationContract]
        bool ProcessRtuInitialized(RtuInitializedDto result);

        [OperationContract]
        bool ConfirmStartMonitoring(MonitoringStartedDto result);

        [OperationContract]
        bool ConfirmStopMonitoring(MonitoringStoppedDto result);

        [OperationContract]
        bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto result);

        [OperationContract]
        bool ConfirmBaseRefAssigned(BaseRefAssignedDto result);


        // RTU notifies

        [OperationContract]
        bool KnowRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto monitoringStep);

        [OperationContract]
        bool ProcessRtuChecksChannel(RtuChecksChannelDto result);

        [OperationContract]
        bool ProcessMonitoringResult(MonitoringResultDto result);

    }
}
