using System;
using System.Collections.Generic;
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
        Task<string> SendCommand(string json, string username, string clientIp);

        [OperationContract]
        Task<string[]> GetEvents(int revision);

        // C2Database
        [OperationContract]
        Task<List<User>> GetUsersAsync();

        [OperationContract]
        Task<List<Zone>> GetZonesAsync();

        [OperationContract]
        Task<MeasurementsList> GetOpticalEvents();

        [OperationContract]
        Task<NetworkEventsList> GetNetworkEvents();

        [OperationContract]
        Task<BopNetworkEventsList> GetBopNetworkEvents();

        [OperationContract]
        Task<TraceStatistics> GetTraceStatistics(Guid traceId);

        [OperationContract]
        Task<byte[]> GetSorBytesOfBase(Guid baseRefId);

        [OperationContract]
        Task<byte[]> GetSorBytes(int sorFileId);

        [OperationContract]
        Task<byte[]> GetSorBytesOfLastTraceMeasurement(Guid traceId);

        [OperationContract]
        Task<MeasurementWithSor> GetLastMeasurementForTrace(Guid traceId);

        [OperationContract]
        Task<MeasurementUpdatedDto> SaveMeasurementChanges(UpdateMeasurementDto dto);

        [OperationContract]
        Task<List<BaseRefDto>> GetTraceBaseRefsAsync(Guid traceId);

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
        Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto baseRefs);

        [OperationContract]
        Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto);

        [OperationContract]
        Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto);
    }
}