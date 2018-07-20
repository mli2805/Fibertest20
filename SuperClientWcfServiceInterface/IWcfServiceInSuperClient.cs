using System.ServiceModel;
using System.Threading.Tasks;

namespace Iit.Fibertest.SuperClientWcfServiceInterface
{
    [ServiceContract]
    public interface IWcfServiceInSuperClient
    {
        [OperationContract]
        Task<int> ClientLoaded(int postfix);
    }
}