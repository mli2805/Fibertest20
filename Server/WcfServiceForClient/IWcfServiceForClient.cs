using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfServiceForClientInterface
{
    [ServiceContract]
    public interface IWcfServiceForClient
    {
        [OperationContract]
        Task<string> SendCommandAsObj(object cmd);

        [OperationContract]
        Task<string> SendCommand(string json);

        [OperationContract]
        Task<string[]> GetEvents(int revision);

        // C2Database
        [OperationContract]
        Task<OpticalEventsList> GetOpticalEvents(int revision);

        [OperationContract]
        Task<NetworkEventsList> GetNetworkEvents(int revision);

        [OperationContract]
        Task<TraceStatistics> GetTraceStatistics(Guid traceId);

        [OperationContract]
        Task<byte[]> GetSorBytesOfBase(Guid baseRefId);

        [OperationContract]
        Task<byte[]> GetSorBytesOfMeasurement(int sorFileId);

        // C2D
        [OperationContract]
        Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto);

        [OperationContract]
        Task UnregisterClientAsync(UnRegisterClientDto dto);

        [OperationContract]
        Task<bool> CheckServerConnection(CheckServerConnectionDto dto);


        // C2D2R
        [OperationContract]
        Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto rtuAddress);

        [OperationContract]
        Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto);

        [OperationContract]
        Task<bool> StartMonitoringAsync(StartMonitoringDto dto);

        [OperationContract]
        Task<bool> StopMonitoringAsync(StopMonitoringDto dto);

        [OperationContract]
        Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefDto baseRef);
    }
}