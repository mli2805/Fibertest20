using System.ServiceModel;
using System.Threading.Tasks;

namespace Iit.Fibertest.SuperClientWcfServiceInterface
{
    [ServiceContract]
    public interface IWcfServiceInSuperClient
    {
        [OperationContract]
        Task<int> ClientLoadingResult(int postfix, bool isLoadedOk, bool isStateOk);
        
        [OperationContract]
        Task<int> ClientClosed(int postfix);

        [OperationContract]
        Task<int> SetSystemState(int postfix, bool isStateOk);
    }
}