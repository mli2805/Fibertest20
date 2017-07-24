using System.ServiceModel;
using Dto;

namespace RtuWcfServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRtuWcfService" in both code and config file together.
    [ServiceContract]
    public interface IRtuWcfService
    {
        [OperationContract]
        string ShakeHandsWithWatchdog(string hello);

        [OperationContract]
        bool Initialize(InitializeRtuDto rtu);

        [OperationContract]
        void StartMonitoring();

        [OperationContract]
        void StopMonitoring();

        [OperationContract]
        bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        bool AssignBaseRef(AssignBaseRefDto baseRef);
    }

}
