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
        bool CheckRtuConnection(NetAddressDto rtuAddress);

        [OperationContract]
        bool InitializeRtu(InitializeRtuDto rtu);

        [OperationContract]
        bool StartMonitoring(string rtuAddress);

        [OperationContract]
        bool StopMonitoring(string rtuAddress);
    }
 
}
