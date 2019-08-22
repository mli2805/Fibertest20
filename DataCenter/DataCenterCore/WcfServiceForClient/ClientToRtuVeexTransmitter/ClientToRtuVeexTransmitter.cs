using System.Threading.Tasks;
using HttpLib;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class ClientToRtuVeexTransmitter
    {
        private readonly IMyLog _logFile;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2RHttpManager _d2RHttpManager;
        private readonly DoubleAddress _serverDoubleAddress;
        public ClientToRtuVeexTransmitter(IniFile iniFile, IMyLog logFile, RtuStationsRepository rtuStationsRepository,
                D2RHttpManager d2RHttpManager)
        {
            _logFile = logFile;
            _rtuStationsRepository = rtuStationsRepository;
            _d2RHttpManager = d2RHttpManager;

            _serverDoubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }

        public async Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent initialize VeEX RTU {dto.RtuId.First6()} request");

            dto.ServerAddresses = _serverDoubleAddress;

            _d2RHttpManager.Initialize(dto.RtuAddresses, _logFile);
            var rtuInitializedDto = await _d2RHttpManager.GetSettings(dto);
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
