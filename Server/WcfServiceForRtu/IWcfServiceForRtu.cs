using System.ServiceModel;
using Dto;

namespace WcfServiceForRtu
{
    [ServiceContract]
    public interface IWcfServiceForRtu
    {
        [OperationContract]
        bool ProcessRtuInitialized(RtuInitialized result);

        [OperationContract]
        bool ConfirmStartMonitoring(MonitoringStarted result);

        [OperationContract]
        bool ConfirmStopMonitoring(MonitoringStopped result);


        [OperationContract]
        bool ProcessMonitoringResult(MonitoringResult result);
    }
}
