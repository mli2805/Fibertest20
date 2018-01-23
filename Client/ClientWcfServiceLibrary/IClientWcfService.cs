using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.ClientWcfServiceInterface
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IClientWcfService" in both code and config file together.
    [ServiceContract]
    public interface IClientWcfService
    {

        // Notifications
        [OperationContract]
        Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto);

        [OperationContract]
        Task<int> NotifyAboutMonitoringResult(MeasurementWithSor dto);

        [OperationContract]
        Task<int> NotifyAboutNetworkEvents(List<NetworkEvent> dto);

        [OperationContract]
        Task<int> NotifyAboutMeasurementClientDone(ClientMeasurementDoneDto dto);
    }


}
