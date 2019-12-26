using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class ClientToRtuVeexTransmitter
    {
        private readonly IMyLog _logFile;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2RtuVeex _d2RtuVeex;
        private readonly D2RtuVeexMonitoring _d2RtuVeexMonitoring;
        private readonly DoubleAddress _serverDoubleAddress;
        public ClientToRtuVeexTransmitter(IniFile iniFile, IMyLog logFile, RtuStationsRepository rtuStationsRepository,
                 D2RtuVeex d2RtuVeex, D2RtuVeexMonitoring d2RtuVeexMonitoring)
        {
            _logFile = logFile;
            _rtuStationsRepository = rtuStationsRepository;
            _d2RtuVeex = d2RtuVeex;
            _d2RtuVeexMonitoring = d2RtuVeexMonitoring;

            _serverDoubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }

        public async Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent initialize VeEX RTU {dto.RtuId.First6()} request");

            dto.ServerAddresses = _serverDoubleAddress;

//            _d2RHttpManager.Initialize(dto.RtuAddresses, _logFile);
            var rtuInitializedDto = await _d2RtuVeex.GetSettings(dto);
//            var rtuInitializedDto = await _d2RHttpManager.GetSettings(dto);
            if (rtuInitializedDto.IsInitialized)
            {
                rtuInitializedDto.RtuAddresses = dto.RtuAddresses;
                var rtuStation = RtuStationFactory.Create(rtuInitializedDto);
                await _rtuStationsRepository.RegisterRtuAsync(rtuStation);
            }

            var message = rtuInitializedDto.IsInitialized
                ? "RTU initialized successfully, monitoring mode is " + (rtuInitializedDto.IsMonitoringOn ? "AUTO" : "MANUAL")
                : "RTU initialization failed";
            _logFile.AppendLine(message);

            return rtuInitializedDto;
        }
    }
}
