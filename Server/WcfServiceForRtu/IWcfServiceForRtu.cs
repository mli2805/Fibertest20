using System.ServiceModel;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfServiceForRtuInterface
{
    [ServiceContract]
    public interface IWcfServiceForRtu
    {
        [OperationContract]
        void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto monitoringStep);

        [OperationContract]
        void RegisterRtuHeartbeat(RtuChecksChannelDto result);
       
        [OperationContract]
        void TransmitClientMeasurementResult(ClientMeasurementDoneDto result);

        [OperationContract]
        void NotifyUserBopStateChanged(BopStateChangedDto dto);
    }
}
