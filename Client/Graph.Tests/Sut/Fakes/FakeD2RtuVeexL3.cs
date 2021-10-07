using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.Dto;

namespace Graph.Tests
{
    public class FakeD2RtuVeexL3 : ID2RtuVeexL3111
    {
        public Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
        {
            return Task.FromResult(new RtuInitializedDto()
            {
                RtuId = dto.RtuId,
                IsInitialized = true,
                ReturnCode = ReturnCode.RtuInitializedSuccessfully,
                Serial = @"123456",
                FullPortCount = 8,
                OwnPortCount = 8,
                RtuAddresses = dto.RtuAddresses,
                OtdrAddress = new NetAddress(dto.RtuAddresses.Main.Ip4Address, 23),
                Version = @"2.0.1.0",
                Children = dto.Children ?? new Dictionary<int, OtauDto>(),
                IsMonitoringOn = false,
                AcceptableMeasParams = new TreeOfAcceptableMeasParams()
                {
                    Units = new Dictionary<string, BranchOfAcceptableMeasParams>() { { @"SM1625", new BranchOfAcceptableMeasParams() } },
                },
            });
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
            return Task.FromResult(new BaseRefAssignedDto()
            {
                ReturnCode = ReturnCode.BaseRefAssignedSuccessfully,
                VeexTests = new List<VeexTestCreatedDto>()
                {
                    new VeexTestCreatedDto(){TestId = Guid.NewGuid()},
                    new VeexTestCreatedDto(){TestId = Guid.NewGuid()}
                }
            });
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