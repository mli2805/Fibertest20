using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class DcManager
    {
        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
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
                await _rtuRegistrationManager.RegisterRtuAsync(rtuInitializedDto);
            }
            return rtuInitializedDto;
        }

        public async Task<bool> StartMonitoringAsync(StartMonitoringDto dto)
        {
            OldRtuStation oldRtuStation;
            if (!_rtuStations.TryGetValue(dto.RtuId, out oldRtuStation))
                return false;

            return await new D2RWcfManager(oldRtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile)
                .StartMonitoringAsync(dto);
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            OldRtuStation oldRtuStation;
            if (!_rtuStations.TryGetValue(dto.RtuId, out oldRtuStation))
                return false;

            return await new D2RWcfManager(oldRtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile)
                .StopMonitoringAsync(dto);
        }

        public async Task<bool> AssignBaseRefAsync(AssignBaseRefDto dto)
        {
            OldRtuStation oldRtuStation;
            if (!_rtuStations.TryGetValue(dto.RtuId, out oldRtuStation))
                return false;

            return await new D2RWcfManager(oldRtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile)
                .AssignBaseRefAsync(dto);
        }

        public async Task<bool> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            OldRtuStation oldRtuStation;
            if (!_rtuStations.TryGetValue(dto.RtuId, out oldRtuStation))
                return false;

            return await new D2RWcfManager(oldRtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile)
                .ApplyMonitoringSettingsAsync(dto);
        }




      
    }
}
