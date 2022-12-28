using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.RtuOccupy;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter : IClientToRtuTransmitter
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly RtuOccupations _rtuOccupations;
        private readonly ID2RWcfManager _d2RWcfManager;

        public ClientToRtuTransmitter(IniFile iniFile, IMyLog logFile, ClientsCollection clientsCollection,
            RtuStationsRepository rtuStationsRepository, RtuOccupations rtuOccupations, ID2RWcfManager d2RWcfManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _rtuStationsRepository = rtuStationsRepository;
            _rtuOccupations = rtuOccupations;
            _d2RWcfManager = d2RWcfManager;
        }


        public async Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} check RTU {dto.NetAddress.ToStringA()} connection");
            return await _d2RWcfManager.CheckRtuConnection(dto, _iniFile, _logFile);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            return await _d2RWcfManager
                .SetRtuAddresses(dto.RtuAddresses, _iniFile, _logFile)
                .InitializeAsync(dto);
        }

    }
}