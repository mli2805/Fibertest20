using System;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class ClientToRtuTransmitter
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly RtuStationsRepository _rtuStationsRepository;

        private readonly DoubleAddress _serverDoubleAddress;

        public ClientToRtuTransmitter(IniFile iniFile, IMyLog logFile, RtuStationsRepository rtuStationsRepository)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _rtuStationsRepository = rtuStationsRepository;

            _serverDoubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }

        public RtuConnectionCheckedDto CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            var result = new RtuConnectionCheckedDto() { RtuId = dto.RtuId };
            var backward = new RtuWcfServiceBackward();
            var wcfFactory = new WcfFactory(new DoubleAddress() { Main = dto.NetAddress }, _iniFile, _logFile);
            var rtuConnection = wcfFactory.CreateDuplexRtuConnection(backward);
            result.IsConnectionSuccessfull = rtuConnection != null;
            if (!result.IsConnectionSuccessfull)
                result.IsPingSuccessful = Pinger.Ping(dto.NetAddress.IsAddressSetAsIp ? dto.NetAddress.Ip4Address : dto.NetAddress.HostName);
            return result;
        }

        public async Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto)
        {
            dto.ServerAddresses = _serverDoubleAddress;
            var rtuInitializedDto = await new D2RWcfManager(dto.RtuAddresses, _iniFile, _logFile).InitializeAsync(dto);
            if (rtuInitializedDto.IsInitialized)
            {
                rtuInitializedDto.RtuAddresses = dto.RtuAddresses;
                var rtuStation = RtuStationFactory.Create(rtuInitializedDto);
                await _rtuStationsRepository.RegisterRtuAsync(rtuStation);
            }
            return rtuInitializedDto;
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                {
                    var result = await new D2RWcfManager(rtuAddresses, _iniFile, _logFile).AttachOtauAsync(dto);
                    return result;
                }

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new OtauAttachedDto() {IsAttached = false, ReturnCode = ReturnCode.NoSuchRtu};
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AttachOtauAsync:" + e.Message);
                return new OtauAttachedDto() {IsAttached = false, ReturnCode = ReturnCode.RtuAttachOtauError, ErrorMessage = e.Message};
            }
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                    return await new D2RWcfManager(rtuAddresses, _iniFile, _logFile).DetachOtauAsync(dto);

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.NoSuchRtu };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DetachOtauAsync:" + e.Message);
                return new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.RtuDetachOtauError, ErrorMessage = e.Message };
            }
        }
    
        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                    return await new D2RWcfManager(rtuAddresses, _iniFile, _logFile)
                        .StopMonitoringAsync(dto);

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return false;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("StopMonitoringAsync:" + e.Message);
                return false;
            }
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                {
                    var result = await new D2RWcfManager(rtuAddresses, _iniFile, _logFile)
                        .AssignBaseRefAsync(dto);
                    _logFile.AppendLine($"Assign base ref(s) result is {result.ReturnCode}");
                    return result;
                }

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.DbError };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AssignBaseRefAsync: " + e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.DbError, ExceptionMessage = e.Message };
            }
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                    return await new D2RWcfManager(rtuAddresses, _iniFile, _logFile)
                        .ApplyMonitoringSettingsAsync(dto);

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.DbError };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("ApplyMonitoringSettingsAsync:" + e.Message);
                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.DbError, ExceptionMessage = e.Message };
            }
        }

        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                {
                    var result = await new D2RWcfManager(rtuAddresses, _iniFile, _logFile).DoClientMeasurementAsync(dto);
                    _logFile.AppendLine($"Client's measurement started with code {result.ReturnCode.ToString()}");
                    return result;
                }

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.DbError };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoClientMeasurementAsync:" + e.Message);
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.DbError, ExceptionMessage = e.Message };
            }
        }

        public async Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                    return await new D2RWcfManager(rtuAddresses, _iniFile, _logFile)
                        .DoOutOfTurnPreciseMeasurementAsync(dto);

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new OutOfTurnMeasurementStartedDto() { ReturnCode = ReturnCode.DbError };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoOutOfTurnPreciseMeasurementAsync:" + e.Message);
                return new OutOfTurnMeasurementStartedDto() { ReturnCode = ReturnCode.DbError, ExceptionMessage = e.Message };
            }
        }
    }
}
