using System.ServiceModel;
using Dto;

namespace WcfServiceForClient
{
    [ServiceContract]
    public interface IWcfServiceForClient
    {
        [OperationContract]
        void RegisterClient(string address);

        [OperationContract]
        void UnRegisterClient(string address);

        [OperationContract]
        bool InitializeRtu(InitializeRtu rtu);
    }
 
}
