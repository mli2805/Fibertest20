using System;
using System.Linq;
using Iit.Fibertest.Dto;
using System.Threading.Tasks;
using Autofac;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.Graph.RtuOccupy;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public class WcfIntermediate
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuOccupations _rtuOccupations;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        public WcfIntermediate(ILifetimeScope globalScope, IMyLog logFile,
            ClientsCollection clientsCollection, RtuOccupations rtuOccupations,
            IFtSignalRClient ftSignalRClient,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _rtuOccupations = rtuOccupations;
            _ftSignalRClient = ftSignalRClient;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
        }

        // Web and Desktop clients send different dtos for RTU initialization
        // so separate WCF channels send adapted command to this Intermediate class
        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            var clientStation = _clientsCollection.Get(dto.ConnectionId);
            _logFile.AppendLine($"Client {clientStation} sent initialize RTU {dto.RtuId.First6()} request");

            var username = clientStation?.UserName ?? "unknown user";
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.Initialization, username, out RtuOccupationState currentState))
            {
                return new RtuInitializedDto()
                {
                    RtuId = dto.RtuId,
                    IsInitialized = false,
                    ReturnCode = ReturnCode.RtuIsBusy,
                    RtuOccupationState = currentState,
                };
            }

            var result = dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.InitializeRtuAsync(dto)
                : await _clientToRtuVeexTransmitter.InitializeRtuAsync(dto);

            await _ftSignalRClient.NotifyAll("RtuInitialized", result.ToCamelCaseJson());

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, username, out RtuOccupationState state);

            var rtuInitializationToGraphApplier = _globalScope.Resolve<RtuInitializationToGraphApplier>();
            return await rtuInitializationToGraphApplier.ApplyRtuInitializationResult(dto, result);
        }
    }
}
