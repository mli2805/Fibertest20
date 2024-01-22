using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfConnections
{
    [ServiceContract]
    public interface IWcfServiceCommonC2D
    {
        IWcfServiceCommonC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp);

        // C2D
        [OperationContract]
        Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto);

        [OperationContract]
        Task<RequestAnswer> SetRtuOccupationState(OccupyRtuDto dto);

        [OperationContract]
        Task<RequestAnswer> RegisterHeartbeat(string connectionId);

        [OperationContract]
        Task<int> UnregisterClientAsync(UnRegisterClientDto dto);

        [OperationContract]
        Task<RequestAnswer> DetachTraceAsync(DetachTraceDto dto);


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
        Task<RequestAnswer> AttachTraceAndSendBaseRefs(AttachTraceDto cmd);

        [OperationContract]
        Task<BaseRefAssignedDto> AssignBaseRefAsyncFromMigrator(AssignBaseRefsDto baseRefs);

        [OperationContract]
        Task<CorrectionProgressDto> StartLandmarksCorrection(LandmarksCorrectionDto changesList);
        [OperationContract]
        Task<CorrectionProgressDto> GetLandmarksCorrectionProgress(Guid batchId);

        [OperationContract]
        Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto baseRefs);

        [OperationContract]
        Task<ClientMeasurementStartedDto> StartClientMeasurementAsync(DoClientMeasurementDto dto);

        [OperationContract]
        Task<ClientMeasurementVeexResultDto> GetClientMeasurementAsync(GetClientMeasurementDto dto);

        [OperationContract]
        Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto);

        [OperationContract]
        Task<RequestAnswer> PrepareReflectMeasurementAsync(PrepareReflectMeasurementDto dto);

        [OperationContract]
        Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto);

        [OperationContract]
        Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto);

        [OperationContract]
        Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto);

        [OperationContract]
        Task<string> UpdateMeasurement(string username, UpdateMeasurementDto dto);

        // C2Database
        [OperationContract]
        Task<byte[]> GetSorBytes(int sorFileId);

        [OperationContract]
        Task<RftsEventsDto> GetRftsEvents(int sorFileId);

    }
}
