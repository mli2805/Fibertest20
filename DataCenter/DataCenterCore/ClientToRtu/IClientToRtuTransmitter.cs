using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IClientToRtuTransmitter
    {
        Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto);
        Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto);
        Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto, DoubleAddress rtuDoubleAddress);
        Task<RequestAnswer> StopMonitoringAsync(StopMonitoringDto dto, DoubleAddress rtuDoubleAddress);
        Task<BaseRefAssignedDto> TransmitBaseRefsToRtuAsync(AssignBaseRefsDto dto, DoubleAddress rtuDoubleAddress);

        Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto);
        Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto);
        Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto);

        //Task<LineParametersDto> GetLineParameters(GetLineParametersDto dto);
        Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto);
        Task<ClientMeasurementVeexResultDto> GetMeasurementClientResultAsync(GetClientMeasurementDto dto);
        Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto);
    }
}