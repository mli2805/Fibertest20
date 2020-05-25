using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfConnections
{
    [ServiceContract]
    public interface IWcfServiceInClient
    {
        [OperationContract]
        Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto);

        [OperationContract]
        Task<int> NotifyAboutMeasurementClientDone(SorBytesDto dto);


        [OperationContract]
        Task<int> AskClientToExit();


        [OperationContract]
        Task<int> BlockClientWhileDbOptimization(DbOptimizationProgressDto dto);

        [OperationContract]
        Task<int> UnBlockClientAfterDbOptimization();
    }
}
