using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public interface ID2RWcfManager
    {
        ID2RWcfManager SetRtuAddresses(DoubleAddress rtuAddress, IniFile iniFile, IMyLog logFile);

        Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto, IniFile iniFile, IMyLog logFile);
        Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto);
        Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto);
        Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto);
        Task<bool> StopMonitoringAsync(StopMonitoringDto dto);
        Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto);
        Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto);
        Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto);
        Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto);
    }
}