using System.ServiceModel;
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
        void RegisterClient(RegisterClientDto dto);

        [OperationContract]
        void UnRegisterClient(UnRegisterClientDto dto);

        [OperationContract]
        bool CheckServerConnection(CheckServerConnectionDto dto);


        // C2D2R
        [OperationContract]
        bool CheckRtuConnection(CheckRtuConnectionDto rtuAddress);

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
