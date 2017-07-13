using System.ServiceModel;
using Dto;

namespace WcfServiceForRtu
{
    [ServiceContract]
    public interface IWcfServiceForRtu
    {
        [OperationContract]
        bool ProcessRtuInitialized(RtuInitializedDto result);

        [OperationContract]
        bool ConfirmStartMonitoring(MonitoringStartedDto result);

        [OperationContract]
        bool ConfirmStopMonitoring(MonitoringStoppedDto result);


        [OperationContract]
        bool ProcessMonitoringResult(MonitoringResult result);
    }
}
