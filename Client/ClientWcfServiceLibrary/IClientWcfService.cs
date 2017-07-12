using System.ServiceModel;
using Dto;

namespace Client_WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IClientWcfService" in both code and config file together.
    [ServiceContract]
    public interface IClientWcfService
    {
        [OperationContract]
        void ConfirmRtuInitialized(RtuInitialized rtu);

        [OperationContract]
        void ConfirmMonitoringStarted(MonitoringStarted confirm);

        [OperationContract]
        void ConfirmMonitoringStopped(MonitoringStopped confirm);
    }


}
