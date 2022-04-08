using System.ServiceModel;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfConnections
{
    [ServiceContract]
    public interface IWcfServiceForRtu
    {
        [OperationContract]
        void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto monitoringStep);

        [OperationContract]
        void RegisterRtuHeartbeat(RtuChecksChannelDto result);
       
        [OperationContract]
        void TransmitClientMeasurementResult(ClientMeasurementResultDto result);
    }
}
