using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IClientToRtuTransmitter
    {
        Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto);
        Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto);
        Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto);
        Task<bool> StopMonitoringAsync(StopMonitoringDto dto);
        Task<BaseRefAssignedDto> TransmitBaseRefsToRtuAsync(AssignBaseRefsDto dto);

        Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto);
        Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto);
        Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto);

        Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto);
        Task<ClientMeasurementVeexResultDto> GetMeasurementClientResultAsync(GetClientMeasurementDto dto);
        Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto);
    }
}