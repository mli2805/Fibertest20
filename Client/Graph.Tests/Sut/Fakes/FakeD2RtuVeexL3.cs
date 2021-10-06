using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.Dto;

namespace Graph.Tests
{
    public class FakeD2RtuVeexL3 : ID2RtuVeexL3
    {
        public Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
        {
            return Task.FromResult(new RtuInitializedDto()
                {ReturnCode = ReturnCode.RtuInitializedSuccessfully, IsInitialized = true});
        }

        public Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto, DoubleAddress rtuDoubleAddress)
        {
            return Task.FromResult(new OtauAttachedDto()
            {
                ReturnCode = ReturnCode.OtauAttachedSuccesfully,
                IsAttached = true,
                RtuId = dto.RtuId,
                // OtauId = Guid.Parse(otau.id),
                // PortCount = otau.portCount,
                // Serial = otau.serialNumber,
            });
        }

        public Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto, DoubleAddress rtuDoubleAddress)
        {
            return Task.FromResult(new OtauDetachedDto()
            {
                IsDetached = true,
                RtuId = dto.RtuId,
                OtauId = dto.OtauId,
                ReturnCode = ReturnCode.OtauDetachedSuccesfully,
            });
        }

        public Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto, DoubleAddress rtuAddresses)
        {
            return Task.FromResult(new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully });
        }

        public Task<bool> StopMonitoringAsync(DoubleAddress rtuAddresses, string otdrId)
        {
            return Task.FromResult(true);
        }

        public Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto, DoubleAddress rtuAddresses)
        {
            return Task.FromResult(new MonitoringSettingsAppliedDto());
        }

        public Task<ClientMeasurementStartedDto> StartMeasurementClient(DoubleAddress rtuDoubleAddress, DoClientMeasurementDto dto)
        {
            return Task.FromResult(new ClientMeasurementStartedDto());
        }

        public Task<ClientMeasurementDto> GetMeasurementClientResult(DoubleAddress rtuDoubleAddress, string measId)
        {
            return Task.FromResult(new ClientMeasurementDto());
        }

        public Task<RequestAnswer> PrepareReflectMeasurementAsync(DoubleAddress rtuDoubleAddress, PrepareReflectMeasurementDto dto)
        {
            return Task.FromResult(new RequestAnswer());
        }

        public Task<MonitoringResultDto> GetTestLastMeasurement(DoubleAddress rtuAddresses, VeexNotificationEvent notificationEvent, bool isFast)
        {
            return Task.FromResult(new MonitoringResultDto());
        }

        public Task<HttpRequestResult> GetCompletedTestsAfterTimestamp(DoubleAddress rtuDoubleAddress, string timestamp, int limit)
        {
            return Task.FromResult(new HttpRequestResult());
        }

        public Task<HttpRequestResult> GetCompletedTestSorBytes(DoubleAddress rtuDoubleAddress, string measId)
        {
            return Task.FromResult(new HttpRequestResult());
        }
    }
}