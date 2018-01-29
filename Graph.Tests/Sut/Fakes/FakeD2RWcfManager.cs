using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Graph.Tests
{
    public class FakeD2RWcfManager : ID2RWcfManager
    {
        public ID2RWcfManager Initialize(DoubleAddress rtuAddress, IniFile iniFile, IMyLog logFile)
        {
            return this;
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
            throw new NotImplementedException();
        }

        public Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            throw new NotImplementedException();
        }
    }
}