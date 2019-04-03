using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfServiceForClientInterface
{
    [ServiceContract]
    public interface IWcfServiceForClient
    {
        void SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp);


        [OperationContract]
        Task<int> SendCommands(List<string> jsons, string username, string clientIp); // especially for Migrator.exe

        [OperationContract]
        Task<int> SendMeas(List<AddMeasurementFromOldBase> list); // especially for Migrator.exe

        [OperationContract]
        Task<int> SendCommandsAsObjs(List<object> cmds);

        [OperationContract]
        Task<string> SendCommandAsObj(object cmd);

        [OperationContract]
        Task<string> SendCommand(string json, string username, string clientIp);

        [OperationContract]
        Task<string[]> GetEvents(GetEventsDto dto);

        // C2Database
        [OperationContract]
        Task<byte[]> GetSorBytes(int sorFileId);

        // C2D
        [OperationContract]
        Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto);

        [OperationContract]
        Task<int> UnregisterClientAsync(UnRegisterClientDto dto);

        [OperationContract]
        Task<bool> CheckServerConnection(CheckServerConnectionDto dto);

        [OperationContract]
        Task<bool> SaveSmtpSettings(SmtpSettingsDto dto);

        [OperationContract]
        Task<bool> SaveGisMode(bool isMapVisible);

        [OperationContract]
        Task<bool> SaveGsmComPort(int comPort);

        [OperationContract]
        Task<bool> SendTest(string to, NotificationType notificationType);

        [OperationContract]
        Task<DiskSpaceDto> GetDiskSpace();

        // C2D2R
        [OperationContract]
        Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto rtuAddress);

        [OperationContract]
        Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto);

        [OperationContract]
        Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto);

        [OperationContract]
        Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto);

        [OperationContract]
        Task<bool> StopMonitoringAsync(StopMonitoringDto dto);

        [OperationContract]
        Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto baseRefs);

        [OperationContract]
        Task<BaseRefAssignedDto> AssignBaseRefAsyncFromMigrator(AssignBaseRefsDto baseRefs);

        [OperationContract]
        Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto baseRefs);

        [OperationContract]
        Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto);

        [OperationContract]
        Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto);
    }
}