using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Graph.Tests
{
    public class FakeD2RWcfManager : ID2RWcfManager
    {
        public ID2RWcfManager SetRtuAddresses(DoubleAddress rtuAddress, IniFile iniFile, IMyLog logFile)
        {
            return this;
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto, IniFile iniFile, IMyLog logFile)
        {
            return Task.FromResult(new RtuConnectionCheckedDto()
            {
                ClientId = dto.ClientId,
                RtuId = dto.RtuId,
                IsConnectionSuccessfull = true,
                IsPingSuccessful = true,
            });
        }

        public Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto)
        {
            return Task.FromResult(new RtuInitializedDto()
            {
                RtuId = dto.RtuId,
                IsInitialized = true,
                ReturnCode = ReturnCode.Ok,
                FullPortCount = 8,
                OwnPortCount = 8,
                Serial = @"123456",
                RtuAddresses = dto.RtuAddresses,
                OtdrAddress =  new NetAddress(dto.RtuAddresses.Main.Ip4Address, 23),
                Version = @"2.0.1.0",
                Children = new Dictionary<int, OtauDto>(),
                IsMonitoringOn = false,
                AcceptableMeasParams = new TreeOfAcceptableMeasParams()
                {
                    Units = new Dictionary<string, BranchOfAcceptableMeasParams>(){{ @"SM1625", new BranchOfAcceptableMeasParams()}},
                },
            });
        }

        public Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            return Task.FromResult(new OtauAttachedDto()
            {
                ReturnCode = ReturnCode.OtauAttachedSuccesfully,
                IsAttached = true,
                RtuId = dto.RtuId,
                OtauId = dto.OtauId,
                PortCount = 16,
                Serial = @"6543210",
            });
        }

        public Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            return Task.FromResult(new OtauDetachedDto()
            {
                ReturnCode = ReturnCode.OtauDetachedSuccesfully,
                RtuId = dto.RtuId,
                OtauId = dto.OtauId,
                IsDetached = true,
            });
        }

        public Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            return Task.FromResult(true);
        }

        public Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            return Task.FromResult(new BaseRefAssignedDto(){ReturnCode = ReturnCode.BaseRefAssignedSuccessfully});
        }

        public Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            return Task.FromResult(new MonitoringSettingsAppliedDto(){ReturnCode = ReturnCode.MonitoringSettingsAppliedSuccessfully});
        }

        public Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            return Task.FromResult(new ClientMeasurementStartedDto(){ReturnCode = ReturnCode.Ok});
        }

        public Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            return Task.FromResult(new OutOfTurnMeasurementStartedDto(){ReturnCode = ReturnCode.Ok});
        }
    }
}