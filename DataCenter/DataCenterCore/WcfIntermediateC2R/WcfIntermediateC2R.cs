using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.Graph.RtuOccupy;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly EventStoreService _eventStoreService;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuOccupations _rtuOccupations;
        private readonly SorFileRepository _sorFileRepository;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly BaseRefsCheckerOnServer _baseRefsChecker;
        private readonly LongOperationsData _longOperationsData;
        private readonly BaseRefRepairmanIntermediary _baseRefRepairmanIntermediary;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;
        private readonly ClientToLinuxRtuHttpTransmitter _clientToLinuxRtuHttpTransmitter;

        private readonly DoubleAddress _serverDoubleAddress;

        public WcfIntermediateC2R(IniFile iniFile, IMyLog logFile,
            Model writeModel, EventStoreService eventStoreService,
            ClientsCollection clientsCollection, RtuOccupations rtuOccupations,
            SorFileRepository sorFileRepository, RtuStationsRepository rtuStationsRepository,
            BaseRefsCheckerOnServer baseRefsChecker, LongOperationsData longOperationsData, 
            BaseRefRepairmanIntermediary baseRefRepairmanIntermediary, IFtSignalRClient ftSignalRClient, 
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter,
            ClientToLinuxRtuHttpTransmitter clientToLinuxRtuHttpTransmitter)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _eventStoreService = eventStoreService;
            _clientsCollection = clientsCollection;
            _rtuOccupations = rtuOccupations;
            _sorFileRepository = sorFileRepository;
            _rtuStationsRepository = rtuStationsRepository;
            _baseRefsChecker = baseRefsChecker;
            _longOperationsData = longOperationsData;
            _baseRefRepairmanIntermediary = baseRefRepairmanIntermediary;
            _ftSignalRClient = ftSignalRClient;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
            _clientToLinuxRtuHttpTransmitter = clientToLinuxRtuHttpTransmitter;

            _serverDoubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }
    }
}
