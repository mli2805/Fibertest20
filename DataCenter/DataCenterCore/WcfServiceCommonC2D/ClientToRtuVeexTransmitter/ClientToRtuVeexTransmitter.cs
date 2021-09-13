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
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2RtuVeexLayer3 _d2RtuVeexLayer3;
        private readonly DoubleAddress _serverDoubleAddress;

        public ClientToRtuVeexTransmitter(IniFile iniFile, IMyLog logFile, Model writeModel,
            RtuStationsRepository rtuStationsRepository, D2RtuVeexLayer3 d2RtuVeexLayer3)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _rtuStationsRepository = rtuStationsRepository;
            _d2RtuVeexLayer3 = d2RtuVeexLayer3;

            _serverDoubleAddress = iniFile.ReadDoubleAddress((int) TcpPorts.ServerListenToRtu);
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto)
        {
            _logFile.AppendLine(
                $"Client from {dto.ClientIp} sent initialize VeEX RTU {dto.RtuId.First6()} request");

            dto.ServerAddresses = _serverDoubleAddress;
            dto.ServerAddresses.Main.Port = (int)TcpPorts.WebApiListenTo;
            if (dto.ServerAddresses.HasReserveAddress)
                dto.ServerAddresses.Reserve.Port = (int)TcpPorts.WebApiListenTo;

            var rtuInitializedDto = await _d2RtuVeexLayer3.InitializeRtu(dto);
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

            var result = await _d2RtuVeexLayer3.ApplyMonitoringSettingsAsync(dto, rtu.OtdrId, rtuAddresses);
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
            await Task.Delay(1);
            return null;
        }

        public async Task<BaseRefAssignedDto> TransmitBaseRefsToRtu(AssignBaseRefsDto dto)
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

            var result = await _d2RtuVeexLayer3.StartMeasurementClient(rtuAddresses, dto);
            _logFile.AppendLine($"Start measurement result is {result.ReturnCode}");
            return result;
        }

        public async Task<ClientMeasurementDto> GetMeasurementResult(GetClientMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {dto.ConnectionId} / {dto.ClientIp} asked to get measurement from VeEX RTU {dto.RtuId.First6()}");
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new ClientMeasurementDto()
                {
                    ReturnCode = ReturnCode.NoSuchRtu
                };
            }

            var result = await _d2RtuVeexLayer3.GetMeasurementClientResult(rtuAddresses, dto.VeexMeasurementId);
            _logFile.AppendLine($"Get measurement result is {result.ReturnCode}");
            return result;
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

        public Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(
            DoOutOfTurnPreciseMeasurementDto dto)
        {
            throw new NotImplementedException();
        }
    }

}
