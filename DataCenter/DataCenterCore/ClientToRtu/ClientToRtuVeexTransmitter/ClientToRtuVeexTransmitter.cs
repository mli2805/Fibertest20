using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class ClientToRtuVeexTransmitter : IClientToRtuTransmitter
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly ClientsCollection _clientsCollection;
        private readonly VeexCompletedTestProcessor _veexCompletedTestProcessor;
        private readonly SorFileRepository _sorFileRepository;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2RtuVeexLayer3 _d2RtuVeexLayer3;

        public ClientToRtuVeexTransmitter(IMyLog logFile, Model writeModel, ClientsCollection clientsCollection,
            VeexCompletedTestProcessor veexCompletedTestProcessor, SorFileRepository sorFileRepository,
            RtuStationsRepository rtuStationsRepository, D2RtuVeexLayer3 d2RtuVeexLayer3)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _clientsCollection = clientsCollection;
            _veexCompletedTestProcessor = veexCompletedTestProcessor;
            _sorFileRepository = sorFileRepository;
            _rtuStationsRepository = rtuStationsRepository;
            _d2RtuVeexLayer3 = d2RtuVeexLayer3;
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            return await _d2RtuVeexLayer3.InitializeRtuAsync(dto);
        }

        public async Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto, DoubleAddress rtuDoubleAddress)
        {
            var result = await _d2RtuVeexLayer3.ApplyMonitoringSettingsAsync(dto, rtuDoubleAddress);
            _logFile.AppendLine($"{result.ReturnCode}");
            if (result.ReturnCode != ReturnCode.MonitoringSettingsAppliedSuccessfully)
                _logFile.AppendLine($"{result.ErrorMessage}");
            return result;
        }

        public async Task<RequestAnswer> StopMonitoringAsync(StopMonitoringDto dto, DoubleAddress rtuDoubleAddress)
        {
            _logFile.AppendLine(
                $"Client {_clientsCollection.Get(dto.ConnectionId)} sent request to stop monitoring on VeEX RTU {dto.RtuId.First6()} ");
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null)
                return new RequestAnswer(ReturnCode.NoSuchRtu);

            var result = await _d2RtuVeexLayer3.StopMonitoringAsync(rtuDoubleAddress, rtu.OtdrId);
            _logFile.AppendLine($"Stop monitoring result is {result}");
            return new RequestAnswer(result ? ReturnCode.Ok : ReturnCode.Error);
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto, DoubleAddress rtuDoubleAddress)
        {
            var result = await _d2RtuVeexLayer3.AttachOtauAsync(dto, rtuDoubleAddress);
            _logFile.AppendLine($"{result.ReturnCode}");
            if (result.ReturnCode != ReturnCode.OtauAttachedSuccessfully)
                _logFile.AppendLine($"{result.ErrorMessage}");
            return result;
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto, DoubleAddress rtuDoubleAddress)
        {
            var result = await _d2RtuVeexLayer3.DetachOtauAsync(dto, rtuDoubleAddress);
            _logFile.AppendLine($"{result.ReturnCode}");
            if (result.ReturnCode != ReturnCode.OtauDetachedSuccessfully)
                _logFile.AppendLine($"{result.ErrorMessage}");
            return result;
        }

        public async Task<BaseRefAssignedDto> TransmitBaseRefsToRtuAsync(AssignBaseRefsDto dto, DoubleAddress rtuDoubleAddress)
        {
            if (dto.BaseRefs.All(b => b.BaseRefType != BaseRefType.Fast))
            {
                var fastDto = await PrepareBaseRefDto(dto, BaseRefType.Fast);
                if (fastDto == null)
                    return new BaseRefAssignedDto()
                    {
                        ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                        ErrorMessage = $"fast base ref not found"
                    };
                dto.BaseRefs.Add(fastDto);
            }

            if (dto.BaseRefs.All(b => b.BaseRefType != BaseRefType.Precise))
            {
                var baseRefDto = await PrepareBaseRefDto(dto, BaseRefType.Precise);
                if (baseRefDto == null)
                    return new BaseRefAssignedDto()
                    {
                        ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                        ErrorMessage = $"precise base ref not found"
                    };
                dto.BaseRefs.Add(baseRefDto);
            }

            var result = await _d2RtuVeexLayer3.AssignBaseRefAsync(dto, rtuDoubleAddress);
            _logFile.AppendLine($"{result.ReturnCode}");
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                _logFile.AppendLine($"{result.ErrorMessage}");
            return result;
        }

        private async Task<BaseRefDto> PrepareBaseRefDto(AssignBaseRefsDto dto, BaseRefType baseRefType)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == dto.TraceId);
            if (trace == null)
                return null;

            var baseRef = _writeModel.BaseRefs.FirstOrDefault(b =>
                b.TraceId == trace.TraceId && b.BaseRefType == baseRefType);
            if (baseRef == null)
                return null;

            var sorBytes = await _sorFileRepository.GetSorBytesAsync(baseRef.SorFileId);
            if (sorBytes == null) return null;

            return baseRef.CreateFromBaseRef(sorBytes);
        }

        public Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto, DoubleAddress rtuDoubleAddress)
        {
            var result = await _d2RtuVeexLayer3.StartMeasurementClientAsync(rtuDoubleAddress, dto);
            _logFile.AppendLine($"Start measurement result is {result.ReturnCode}");
            return result;
        }

        public async Task<ClientMeasurementVeexResultDto> GetMeasurementClientResultAsync(GetClientMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} asked to get measurement {dto.VeexMeasurementId.Substring(0, 6)} from VeEX RTU {dto.RtuId.First6()}");
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new ClientMeasurementVeexResultDto()
                {
                    ReturnCode = ReturnCode.NoSuchRtu
                };
            }

            var result = await _d2RtuVeexLayer3.GetMeasurementClientResultAsync(rtuAddresses, dto.VeexMeasurementId);
            _logFile.AppendLine($"Get measurement result: request is {result.ReturnCode}; measurement status is {result.VeexMeasurementStatus}");
            return result;
        }

        public async Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} asked to get measurement sor bytes from VeEX RTU {dto.RtuId.First6()}");
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return null;
            }

            var res = await _d2RtuVeexLayer3.GetClientMeasurementSorBytesAsync(rtuAddresses, dto.VeexMeasurementId);
            return new ClientMeasurementVeexResultDto()
            {
                ReturnCode = ReturnCode.Ok,
                SorBytes = res.ResponseBytesArray,
            };
        }

        public async Task<RequestAnswer> PrepareReflectMeasurementAsync(PrepareReflectMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {dto.ConnectionId} / {dto.ClientIp} asked to prepare Reflect measurement on VeEX RTU {dto.RtuId.First6()}");
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.NoSuchRtu
                };
            }

            var result = await _d2RtuVeexLayer3.PrepareReflectMeasurementAsync(rtuAddresses, dto);
            _logFile.AppendLine($"Prepare Reflect measurement result is {result.ReturnCode}");
            return result;
        }

        public async Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto, DoubleAddress rtuDoubleAddress)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} asked to start measurement out of turn on VeEX RTU {dto.RtuId.First6()}");
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return new RequestAnswer(ReturnCode.NoSuchRtu);

            var veexTest = _writeModel.VeexTests.FirstOrDefault(v =>
                v.BasRefType == BaseRefType.Precise && v.TraceId == dto.PortWithTraceDto.TraceId);
            if (veexTest == null)
            {
                _logFile.AppendLine($"No precise veex test for RTU {dto.RtuId.First6()} and trace {dto.PortWithTraceDto.TraceId.First6()}");
                return new RequestAnswer { ReturnCode = ReturnCode.NoSuchVeexTest };
            }
            _logFile.AppendLine($"Out of turn measurement for Veex test {veexTest.TestId.First6()} {veexTest.OtauId}");

            var result = await _d2RtuVeexLayer3.StartOutOfTurnPreciseMeasurementAsync(rtuDoubleAddress, rtu.OtdrId, veexTest.TestId.ToString());
            var errorMessage = result.ReturnCode == ReturnCode.Error ? result.ErrorMessage : "";
            var rs = $"result is {result.ReturnCode} {errorMessage}";
            _logFile.AppendLine($"Start out of turn measurement (testId = {veexTest.TestId.First6()}) {rs}");

            if (result.ReturnCode == ReturnCode.Ok)
                _veexCompletedTestProcessor.RequestedTests.TryAdd(veexTest.TestId, dto.ConnectionId);
            return result;
        }
    }

}
