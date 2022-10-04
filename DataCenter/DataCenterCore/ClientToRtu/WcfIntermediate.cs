using System;
using System.Linq;
using Iit.Fibertest.Dto;
using System.Threading.Tasks;
using Autofac;
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
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        public WcfIntermediate(ILifetimeScope globalScope, IMyLog logFile, 
            ClientsCollection clientsCollection, RtuOccupations rtuOccupations,
            IFtSignalRClient ftSignalRClient, 
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter    )
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
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} sent initialize RTU {dto.RtuId.First6()} request");

            var username = _clientsCollection.Clients.FirstOrDefault(u=>u.ConnectionId == dto.ConnectionId)?.UserName ?? "unknown user";
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.Initialization, username, out RtuOccupationState currentState))
            {
                return new RtuInitializedDto()
                {
                    RtuId =  dto.RtuId, 
                    IsInitialized = false,
                    ReturnCode = ReturnCode.RtuIsBusy,
                    RtuOccupationState = currentState,
                };
            }
            
            var result = dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.InitializeRtuAsync(dto)
                : await _clientToRtuVeexTransmitter.InitializeRtuAsync(dto);

            await _ftSignalRClient.NotifyAll("RtuInitialized", result.ToCamelCaseJson());

            await ClearRtuOccupationState(dto.RtuId);

            var rtuInitializationToGraphApplier = _globalScope.Resolve<RtuInitializationToGraphApplier>();
            return await rtuInitializationToGraphApplier.ApplyRtuInitializationResult(dto, result);
        }

        public async Task<RequestAnswer> ClearRtuOccupationState(Guid rtuId)
        {
            await Task.Delay(1);
            _logFile.AppendLine($"Clear  RTU {rtuId.First6()} OccupationState");
            if (!_rtuOccupations.TrySetOccupation(rtuId, RtuOccupation.None, "", out RtuOccupationState currentState))
            {
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.RtuIsBusy,
                    RtuOccupationState = currentState,
                    ErrorMessage = "",
                };
            }

            return new RequestAnswer() { ReturnCode = ReturnCode.Ok };
        }

    }
}
