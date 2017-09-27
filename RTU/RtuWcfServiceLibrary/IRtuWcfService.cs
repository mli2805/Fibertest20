using System.ServiceModel;
using System.Threading.Tasks;
using Dto;

namespace RtuWcfServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRtuWcfService" in both code and config file together.
    [ServiceContract]
    public interface IRtuWcfService
    {
        [OperationContract]
        Task<RtuInitializedDto> InitializeAndAnswer(InitializeRtuDto dto);

        [OperationContract]
        bool Initialize(InitializeRtuDto rtu);

        [OperationContract]
        bool StartMonitoring(StartMonitoringDto dto);

        [OperationContract]
        bool StopMonitoring(StopMonitoringDto dto);

        [OperationContract]
        bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        bool AssignBaseRef(AssignBaseRefDto baseRef);

        [OperationContract]
        bool ToggleToPort(OtauPortDto port);


        // for WatchDog
        [OperationContract]
        bool CheckLastSuccessfullMeasTime();
    }

}
