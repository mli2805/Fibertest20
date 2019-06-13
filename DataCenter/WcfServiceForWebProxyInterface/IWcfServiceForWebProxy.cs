using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Iit.Fibertest.WcfServiceForWebProxyInterface
{
    [ServiceContract]
    public interface IWcfServiceForWebProxy
    {
        [OperationContract]
        Task<List<string>> GetRtuList();
    }
}
