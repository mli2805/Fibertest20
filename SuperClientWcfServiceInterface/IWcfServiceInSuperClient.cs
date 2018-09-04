using System.ServiceModel;
using System.Threading.Tasks;

namespace Iit.Fibertest.SuperClientWcfServiceInterface
{
    [ServiceContract]
    public interface IWcfServiceInSuperClient
    {
        [OperationContract]
        Task<int> ClientLoaded(int postfix, bool isStateOk);
        
        [OperationContract]
        Task<int> ClientClosed(int postfix);

        [OperationContract]
        Task<int> ClientStateChanged(int postfix);
    }
}