using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Graph.Tests
{
    public class FakeD2RWcfManager : ID2RWcfManager
    {
        private ReturnCode _fakeInitializationReturnCode;
        private string _serial;
        private int _ownPortCount;
        private int _fullPortCount;
        private string _waveLength;
        private Dictionary<int, OtauDto> _children;

        public void SetFakeInitializationAnswer(ReturnCode returnCode = ReturnCode.Ok, string serial = "123456",
            int ownPortCount = 8, int fullPortCount = 8, string waveLength = "SM1625", Dictionary<int, OtauDto> children = null)
        {
            _fakeInitializationReturnCode = returnCode;
            _serial = serial;
            _ownPortCount = ownPortCount;
            _fullPortCount = fullPortCount;
            _waveLength = waveLength;
            _children = children;
        }

        public ID2RWcfManager SetRtuAddresses(DoubleAddress rtuAddress, IniFile iniFile, IMyLog logFile)
        {
            return this;
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto, IniFile iniFile, IMyLog logFile)
        {
            return Task.FromResult(new RtuConnectionCheckedDto()
            {
                ClientIp = dto.ClientIp,
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
                ReturnCode = _fakeInitializationReturnCode,
                Serial = _serial,
                FullPortCount = _fullPortCount,
                OwnPortCount = _ownPortCount,
                RtuAddresses = dto.RtuAddresses,
                OtdrAddress = new NetAddress(dto.RtuAddresses.Main.Ip4Address, 23),
                Version = @"2.0.1.0",
                Children = _children ?? new Dictionary<int, OtauDto>(),
                IsMonitoringOn = false,
                AcceptableMeasParams = new TreeOfAcceptableMeasParams()
                {
                    Units = new Dictionary<string, BranchOfAcceptableMeasParams>() { { _waveLength, new BranchOfAcceptableMeasParams() } },
                },
            });
        }

        public Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            return Task.FromResult(new OtauAttachedDto()
            {
                ReturnCode = ReturnCode.OtauAttachedSuccessfully,
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
                ReturnCode = ReturnCode.OtauDetachedSuccessfully,
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
            return Task.FromResult(new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully });
        }

        public Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            return Task.FromResult(new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.MonitoringSettingsAppliedSuccessfully });
        }

        public Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            return Task.FromResult(new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.Ok });
        }

        public Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            return Task.FromResult(new RequestAnswer() { ReturnCode = ReturnCode.Ok });
        }

        public Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto)
        {
            return Task.FromResult(new RequestAnswer() { ReturnCode = ReturnCode.Ok });
        }
    }
}