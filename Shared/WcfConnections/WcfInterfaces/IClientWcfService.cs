using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfConnections
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IClientWcfService" in both code and config file together.
    [ServiceContract]
    public interface IClientWcfService
    {
        [OperationContract]
        Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto);

        [OperationContract]
        Task<int> NotifyAboutMeasurementClientDone(ClientMeasurementDoneDto dto);


        [OperationContract]
        Task<int> AskClientToExit();


        [OperationContract]
        Task<int> BlockClientWhileDbOptimization(DbOptimizationProgressDto dto);

        [OperationContract]
        Task<int> UnBlockClientAfterDbOptimization();
    }
}
