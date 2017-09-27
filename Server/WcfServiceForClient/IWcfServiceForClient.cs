using System.ServiceModel;
using System.Threading.Tasks;
using Dto;

namespace WcfServiceForClientLibrary
{
    [ServiceContract]
    public interface IWcfServiceForClient
    {
        [OperationContract]
        string SendCommand(string json);
        [OperationContract]
        string[] GetEvents(int revision);

        // C2D
        [OperationContract]
        Task<ClientRegisteredDto> MakeExperimentAsync(RegisterClientDto dto);

        [OperationContract]
        void RegisterClient(RegisterClientDto dto);

        [OperationContract]
        void UnRegisterClient(UnRegisterClientDto dto);

        [OperationContract]
        bool CheckServerConnection(CheckServerConnectionDto dto);


        // C2D2R
        [OperationContract]
        bool CheckRtuConnection(CheckRtuConnectionDto rtuAddress);


        [OperationContract]
        Task<RtuInitializedDto> InitializeRtuLongTask(InitializeRtuDto dto);

        [OperationContract]
        bool InitializeRtu(InitializeRtuDto rtu);

        [OperationContract]
        bool StartMonitoring(StartMonitoringDto dto);

        [OperationContract]
        bool StopMonitoring(StopMonitoringDto dto);

        [OperationContract]
        bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        bool AssignBaseRef(AssignBaseRefDto baseRef);
    }
 
}
