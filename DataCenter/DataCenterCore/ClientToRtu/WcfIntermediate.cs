using System;
using Iit.Fibertest.Dto;
using System.Threading.Tasks;
using Autofac;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.Graph.RtuOccupy;

namespace Iit.Fibertest.DataCenterCore
{
    public class WcfIntermediate
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuOccupations _rtuOccupations;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        private readonly DoubleAddress _serverDoubleAddress;

        public WcfIntermediate(ILifetimeScope globalScope, IniFile iniFile, IMyLog logFile,
            ClientsCollection clientsCollection, RtuOccupations rtuOccupations, RtuStationsRepository rtuStationsRepository,
            IFtSignalRClient ftSignalRClient,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _rtuOccupations = rtuOccupations;
            _rtuStationsRepository = rtuStationsRepository;
            _ftSignalRClient = ftSignalRClient;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;

            _serverDoubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }

        // Web and Desktop clients send different dtos for RTU initialization
        // so separate WCF channels send adapted command to this Intermediate class
        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            var clientStation = _clientsCollection.Get(dto.ConnectionId);
            _logFile.AppendLine($"Client {clientStation} sent initialize RTU {dto.RtuId.First6()} request");

            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.Initialization, clientStation?.UserName, out RtuOccupationState currentState))
            {
                return new RtuInitializedDto()
                {
                    RtuId = dto.RtuId,
                    IsInitialized = false,
                    ReturnCode = ReturnCode.RtuIsBusy,
                    RtuOccupationState = currentState,
                };
            }

            dto.ServerAddresses = (DoubleAddress)_serverDoubleAddress.Clone();
            if (!dto.RtuAddresses.HasReserveAddress)
                // if RTU has no reserve address it should not send to server's reserve address
                // (it is an ideological requirement)
                dto.ServerAddresses.HasReserveAddress = false;

            var rtuInitializedDto = dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.InitializeRtuAsync(dto)
                : await _clientToRtuVeexTransmitter.InitializeRtuAsync(dto);

            await _ftSignalRClient.NotifyAll("RtuInitialized", rtuInitializedDto.ToCamelCaseJson());

            if (rtuInitializedDto.IsInitialized)
            {
                try
                {
                    rtuInitializedDto.RtuAddresses = dto.RtuAddresses;
                    var rtuStation = RtuStationFactory.Create(rtuInitializedDto);
                    await _rtuStationsRepository.RegisterRtuInitializationResultAsync(rtuStation);
                }
                catch (Exception e)
                {
                    rtuInitializedDto.ReturnCode = ReturnCode.Error;
                    rtuInitializedDto.ErrorMessage = $"Failed to save RTU in DB: {e.Message}";
                }
            }

            var message = rtuInitializedDto.IsInitialized
                ? "RTU initialized successfully, monitoring mode is " +
                  (rtuInitializedDto.IsMonitoringOn ? "AUTO" : "MANUAL")
                : "RTU initialization failed";
            _logFile.AppendLine(message);


            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, clientStation?.UserName, out RtuOccupationState _);

            var rtuInitializationToGraphApplier = _globalScope.Resolve<RtuInitializationToGraphApplier>();
            return await rtuInitializationToGraphApplier.ApplyRtuInitializationResult(dto, rtuInitializedDto);
        }
    }
}
