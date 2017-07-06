using System.ServiceModel;
using Dto;

namespace D4C_WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ID4CWcfService" in both code and config file together.
    [ServiceContract]
    public interface ID4CWcfService
    {
        [OperationContract]
        bool InitializeRtu(InitializeRtu rtu);
    }
 
}
