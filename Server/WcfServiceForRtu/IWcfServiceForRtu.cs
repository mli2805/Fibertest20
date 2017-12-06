using System.ServiceModel;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfServiceForRtuInterface
{
    [ServiceContract]
    public interface IWcfServiceForRtu
    {

        // RTU notifies

        [OperationContract]
        void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto monitoringStep);

        [OperationContract]
        void RegisterRtuHeartbeat(RtuChecksChannelDto result);
       
    }
}
