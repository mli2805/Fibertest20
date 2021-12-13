using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfConnections
{
    [ServiceContract]
    public interface IWcfServiceDesktopC2D
    {
        IWcfServiceDesktopC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp);


        [OperationContract]
        Task<bool> SendHeartbeat(HeartbeatDto dto);

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

        [OperationContract]
        Task<SerializedModelDto> GetModelDownloadParams(GetSnapshotDto dto);

        [OperationContract]
        Task<byte[]> GetModelPortion(int portionOrdinal);

        // C2D

        [OperationContract]
        Task<bool> CheckServerConnection(CheckServerConnectionDto dto);

        [OperationContract]
        Task<bool> SaveSmtpSettings(SmtpSettingsDto dto);

        [OperationContract]
        Task<bool> SaveAndTestSnmpSettings(SnmpSettingsDto dto);

        [OperationContract]
        Task<bool> SaveGisMode(bool isMapVisible);

        [OperationContract]
        Task<bool> SaveGsmComPort(int comPort);

        [OperationContract]
        Task<bool> SendTest(string to, NotificationType notificationType);

        [OperationContract]
        Task<DiskSpaceDto> GetDiskSpaceGb();

    }
}