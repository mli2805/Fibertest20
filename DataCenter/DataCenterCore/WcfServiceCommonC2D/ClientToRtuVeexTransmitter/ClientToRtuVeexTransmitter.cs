using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class ClientToRtuVeexTransmitter : IClientToRtuTransmitter
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly VeexCompletedTestProcessor _veexCompletedTestProcessor;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2RtuVeexLayer3 _d2RtuVeexLayer3;

        public ClientToRtuVeexTransmitter(IMyLog logFile, Model writeModel,
            VeexCompletedTestProcessor veexCompletedTestProcessor,
            RtuStationsRepository rtuStationsRepository, D2RtuVeexLayer3 d2RtuVeexLayer3)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _veexCompletedTestProcessor = veexCompletedTestProcessor;
            _rtuStationsRepository = rtuStationsRepository;
            _d2RtuVeexLayer3 = d2RtuVeexLayer3;
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            _logFile.AppendLine(
                $"Client from {dto.ClientIp} sent initialize VeEX RTU {dto.RtuId.First6()} request");

            var rtuInitializedDto = await _d2RtuVeexLayer3.InitializeRtuAsync(dto);
            if (rtuInitializedDto.IsInitialized)
            {
                rtuInitializedDto.RtuAddresses = dto.RtuAddresses;
                var rtuStation = RtuStationFactory.Create(rtuInitializedDto);
                await _rtuStationsRepository.RegisterRtuAsync(rtuStation);
            }

            var message = rtuInitializedDto.IsInitialized
                ? "RTU initialized successfully, monitoring mode is " +
                  (rtuInitializedDto.IsMonitoringOn ? "AUTO" : "MANUAL")
                : "RTU initialization failed";
            _logFile.AppendLine(message);

            return rtuInitializedDto;
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            _logFile.AppendLine(
                $"Client from {dto.ClientIp} sent apply monitoring settings to VeEX RTU {dto.RtuId.First6()} request");
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return null;

            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new MonitoringSettingsAppliedDto()
                {
                    ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError,
                    ErrorMessage = $"Unknown RTU {dto.RtuId.First6()}"
                };
            }

            var result = await _d2RtuVeexLayer3.ApplyMonitoringSettingsAsync(dto, rtuAddresses);
            _logFile.AppendLine($"{result.ReturnCode}");
            if (result.ReturnCode != ReturnCode.MonitoringSettingsAppliedSuccessfully)
                _logFile.AppendLine($"{result.ErrorMessage}");
            return result;
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            _logFile.AppendLine(
                $"Client from {dto.ClientIp} sent request to stop monitoring on VeEX RTU {dto.RtuId.First6()} ");
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return false;

            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return false;
            }

            var result = await _d2RtuVeexLayer3.StopMonitoringAsync(rtuAddresses, rtu.OtdrId);
            _logFile.AppendLine($"Stop monitoring result is {result}");
            return result;
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new OtauAttachedDto()
                {
                    ReturnCode = ReturnCode.RtuAttachOtauError,
                    ErrorMessage = $"Unknown RTU {dto.RtuId.First6()}"
                };
            }

            var result = await _d2RtuVeexLayer3.AttachOtauAsync(dto, rtuAddresses);
            _logFile.AppendLine($"{result.ReturnCode}");
            if (result.ReturnCode != ReturnCode.OtauAttachedSuccessfully)
                _logFile.AppendLine($"{result.ErrorMessage}");
            return result;
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new OtauDetachedDto()
                {
                    ReturnCode = ReturnCode.RtuDetachOtauError,
                    ErrorMessage = $"Unknown RTU {dto.RtuId.First6()}"
                };
            }

            var result = await _d2RtuVeexLayer3.DetachOtauAsync(dto, rtuAddresses);
            _logFile.AppendLine($"{result.ReturnCode}");
            if (result.ReturnCode != ReturnCode.OtauDetachedSuccessfully)
                _logFile.AppendLine($"{result.ErrorMessage}");
            return result;
        }

        public async Task<BaseRefAssignedDto> TransmitBaseRefsToRtuAsync(AssignBaseRefsDto dto)
        {
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new BaseRefAssignedDto()
                {
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                    ErrorMessage = $"Unknown RTU {dto.RtuId.First6()}"
                };
            }

            var result = await _d2RtuVeexLayer3.AssignBaseRefAsync(dto, rtuAddresses);
            _logFile.AppendLine($"{result.ReturnCode}");
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                _logFile.AppendLine($"{result.ErrorMessage}");
            return result;
        }

        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {dto.ConnectionId} / {dto.ClientIp} asked to do measurement on VeEX RTU {dto.RtuId.First6()}");
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new ClientMeasurementStartedDto()
                {
                    ReturnCode = ReturnCode.NoSuchRtu
                };
            }

            var result = await _d2RtuVeexLayer3.StartMeasurementClientAsync(rtuAddresses, dto);
            _logFile.AppendLine($"Start measurement result is {result.ReturnCode}");
            return result;
        }

        public async Task<ClientMeasurementVeexResultDto> GetMeasurementClientResultAsync(GetClientMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {dto.ConnectionId} / {dto.ClientIp} asked to get measurement from VeEX RTU {dto.RtuId.First6()}");
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
            _logFile.AppendLine($"Get measurement result is {result.ReturnCode}");
            return result;
        }  
        
        public async Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {dto.ConnectionId} / {dto.ClientIp} asked to get measurement sor bytes from VeEX RTU {dto.RtuId.First6()}");
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

        public async Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(
            DoOutOfTurnPreciseMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {dto.ConnectionId} / {dto.ClientIp} asked to start measurement out of turn on VeEX RTU {dto.RtuId.First6()}");
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.NoSuchRtu
                };
            }

            var veexTest = _writeModel.VeexTests.FirstOrDefault(v =>
                v.BasRefType == BaseRefType.Precise && v.TraceId == dto.PortWithTraceDto.TraceId);
            if (veexTest == null)
            {
                _logFile.AppendLine($"No precise veex test for RTU {dto.RtuId.First6()} and trace {dto.PortWithTraceDto.TraceId.First6()}");
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.NoSuchRtu
                };
            }

            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null)
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.NoSuchRtu
                };

            var result = await _d2RtuVeexLayer3.StartOutOfTurnPreciseMeasurementAsync(rtuAddresses, rtu.OtdrId, veexTest.TestId.ToString());
            _logFile.AppendLine($"Start out of turn measurement result is {result.ReturnCode}");
            if (result.ReturnCode == ReturnCode.Ok)
                _veexCompletedTestProcessor.RequestedTests.TryAdd(veexTest.TestId, dto.ConnectionId);
            return result;
        }
    }

}
