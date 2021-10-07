using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public interface ID2RtuVeexL3111
    {
        Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto);

        Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto, DoubleAddress rtuDoubleAddress);
        Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto, DoubleAddress rtuDoubleAddress);

        Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto, DoubleAddress rtuAddresses);

        Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto,
            DoubleAddress rtuAddresses);
        Task<bool> StopMonitoringAsync(DoubleAddress rtuAddresses, string otdrId);

        Task<ClientMeasurementStartedDto> StartMeasurementClient(DoubleAddress rtuDoubleAddress,
            DoClientMeasurementDto dto);
        Task<ClientMeasurementDto> GetMeasurementClientResult(DoubleAddress rtuDoubleAddress, string measId);

        Task<RequestAnswer> PrepareReflectMeasurementAsync(DoubleAddress rtuDoubleAddress,
            PrepareReflectMeasurementDto dto);

        Task<MonitoringResultDto> GetTestLastMeasurement(DoubleAddress rtuAddresses,
            VeexNotificationEvent notificationEvent, bool isFast);
        Task<HttpRequestResult> GetCompletedTestsAfterTimestamp(DoubleAddress rtuDoubleAddress,
            string timestamp, int limit);
        Task<HttpRequestResult> GetCompletedTestSorBytes(DoubleAddress rtuDoubleAddress, string measId);

    }
}
