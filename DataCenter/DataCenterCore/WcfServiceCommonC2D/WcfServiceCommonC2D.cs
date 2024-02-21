using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.RtuOccupy;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class WcfServiceCommonC2D : IWcfServiceCommonC2D
    {
        private readonly GlobalState _globalState;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly SorFileRepository _sorFileRepository;
        private readonly EventStoreService _eventStoreService;
        private readonly ClientsCollection _clientsCollection;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly RtuOccupations _rtuOccupations;
        private readonly WcfIntermediateC2R _wcfIntermediateC2R;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;
        private readonly ClientToLinuxRtuHttpTransmitter _clientToLinuxRtuHttpTransmitter;

        public WcfServiceCommonC2D(GlobalState globalState, IMyLog logFile,
            Model writeModel, SorFileRepository sorFileRepository,
            EventStoreService eventStoreService, ClientsCollection clientsCollection,
            BaseRefLandmarksTool baseRefLandmarksTool,
            IFtSignalRClient ftSignalRClient, RtuOccupations rtuOccupations,
            WcfIntermediateC2R wcfIntermediateC2R,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter,
            ClientToLinuxRtuHttpTransmitter clientToLinuxRtuHttpTransmitter)
        {
            _globalState = globalState;
            _logFile = logFile;
            _writeModel = writeModel;
            _sorFileRepository = sorFileRepository;
            _eventStoreService = eventStoreService;
            _clientsCollection = clientsCollection;
            _baseRefLandmarksTool = baseRefLandmarksTool;
            _ftSignalRClient = ftSignalRClient;
            _rtuOccupations = rtuOccupations;
            _wcfIntermediateC2R = wcfIntermediateC2R;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
            _clientToLinuxRtuHttpTransmitter = clientToLinuxRtuHttpTransmitter;
        }

        public IWcfServiceCommonC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            return this;
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            if (_globalState.IsDatacenterInDbOptimizationMode)
                return new ClientRegisteredDto { ReturnCode = ReturnCode.Error };

            var result = await _clientsCollection.RegisterClientAsync(dto);
            result.StreamIdOriginal = _eventStoreService.StreamIdOriginal;
            result.SnapshotLastEvent = _eventStoreService.LastEventNumberInSnapshot;
            result.SnapshotLastDate = _eventStoreService.LastEventDateInSnapshot;

            var command = new RegisterClientStation { RegistrationResult = result.ReturnCode };
            await _eventStoreService.SendCommand(command, dto.UserName, dto.ClientIp);

            return result;
        }

        public async Task<RequestAnswer> SetRtuOccupationState(OccupyRtuDto dto)
        {
            await Task.Delay(1);
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, dto.State.RtuOccupation, dto.State.UserName, out RtuOccupationState currentState))
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

        public async Task<RequestAnswer> RegisterHeartbeat(string connectionId)
        {
            await Task.Delay(1);
            var result = _clientsCollection.RegisterHeartbeat(connectionId);
            return result
                ? new RequestAnswer() { ReturnCode = ReturnCode.Ok, ErrorMessage = "OK" }
                : new RequestAnswer() { ReturnCode = ReturnCode.ClientCleanedAsDead, ErrorMessage = "Client not found." };
        }

        public async Task<int> UnregisterClientAsync(UnRegisterClientDto dto)
        {
            if (_clientsCollection.UnregisterClientAsync(dto))
            {
                var command = new UnregisterClientStation();
                await _eventStoreService.SendCommand(command, dto.Username, dto.ClientIp);
            }
            return 0;
        }

        public async Task<RequestAnswer> DetachTraceAsync(DetachTraceDto dto)
        {
            var rtuId = _writeModel.Traces.First(t => t.TraceId == dto.TraceId).RtuId;
            var clientStation = _clientsCollection.Get(dto.ConnectionId);
            var username = clientStation?.UserName;
            if (!_rtuOccupations.TrySetOccupation(rtuId, RtuOccupation.DetachTraces, username, out RtuOccupationState _))
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.RtuIsBusy
                };

            var command = new DetachTrace() { TraceId = dto.TraceId };
            var result = await _eventStoreService.SendCommand(command, username, clientStation?.ClientIp);
            await _ftSignalRClient.NotifyAll("FetchTree", null);

            _rtuOccupations.TrySetOccupation(rtuId, RtuOccupation.None, username, out RtuOccupationState _);

            return !string.IsNullOrEmpty(result)
                ? new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = result }
                : new RequestAnswer() { ReturnCode = ReturnCode.Ok };
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"CheckRtuConnectionAsync: {dto.NetAddress.ToStringA()}");
            switch (dto.NetAddress.Port)
            {
                case (int)TcpPorts.RtuListenTo: return await _clientToRtuTransmitter.CheckRtuConnection(dto);
                case (int)TcpPorts.RtuVeexListenTo:
                    // TODO separate checker for VEEX RTU
                    return await _clientToRtuTransmitter.CheckRtuConnection(dto);
                case (int)TcpPorts.RtuListenToHttp:
                    return await _clientToLinuxRtuHttpTransmitter.CheckRtuConnection(dto);
                // case -1: - new RTU with port not set
                default:
                    return await CheckRtuConnectionFirstTime(dto);
            }
        }

        private async Task<RtuConnectionCheckedDto> CheckRtuConnectionFirstTime(CheckRtuConnectionDto dto)
        {
            dto.NetAddress.Port = (int)TcpPorts.RtuListenToHttp; // МАК linux
            var result = await _clientToLinuxRtuHttpTransmitter.CheckRtuConnection(dto);
            if (result.IsConnectionSuccessfull) return result;

            dto.NetAddress.Port = (int)TcpPorts.RtuListenTo;
            result = await _clientToRtuTransmitter.CheckRtuConnection(dto);
            if (result.IsConnectionSuccessfull) return result;

            dto.NetAddress.Port = (int)TcpPorts.RtuVeexListenTo;
            //TODO implement special checker for VEEX RTU
            return await _clientToRtuTransmitter.CheckRtuConnection(dto);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            return await _wcfIntermediateC2R.InitializeRtuAsync(dto);
        }

        public async Task<RtuCurrentStateDto> GetRtuCurrentState(GetCurrentRtuStateDto dto)
        {
            var state = await _clientToLinuxRtuHttpTransmitter.GetRtuCurrentState(dto);
            _logFile.AppendLine($"state initialization is null - {state.LastInitializationResult == null}");
            return state;
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            return await _wcfIntermediateC2R.AttachOtauAsync(dto);
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            return await _wcfIntermediateC2R.DetachOtauAsync(dto);
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var stopResult = await _wcfIntermediateC2R.StopMonitoringAsync(dto);
            return stopResult.ReturnCode == ReturnCode.Ok;
        }

        public async Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            return await _wcfIntermediateC2R.ApplyMonitoringSettingsAsync(dto);
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            return await _wcfIntermediateC2R.AssignBaseRefAsync(dto);
        }

        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        public async Task<RequestAnswer> AttachTraceAndSendBaseRefs(AttachTraceDto dto)
        {
            return await _wcfIntermediateC2R.AttachTraceAndSendBaseRefs(dto);
        }

        public async Task<CorrectionProgressDto> StartLandmarksCorrection(LandmarksCorrectionDto changesList)
        {
            return await _wcfIntermediateC2R.StartLandmarksCorrection(changesList);
        }

        public async Task<CorrectionProgressDto> GetLandmarksCorrectionProgress(Guid batchId)
        {
            return await _wcfIntermediateC2R.GetLandmarksCorrectionProgress(batchId);
        }

        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        // or user explicitly demands to resend base refs to RTU 
        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            return await _wcfIntermediateC2R.ReSendBaseRefAsync(dto);
        }

        public async Task<ClientMeasurementStartedDto> StartClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            return await _wcfIntermediateC2R.DoClientMeasurementAsync(dto);
            // Client must free RTU when result received
        }

        public async Task<ClientMeasurementVeexResultDto> GetClientMeasurementAsync(GetClientMeasurementDto dto)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return new ClientMeasurementVeexResultDto() { ReturnCode = ReturnCode.NoSuchRtu };

            return rtu.RtuMaker == RtuMaker.VeEX
                ? await _clientToRtuVeexTransmitter.GetMeasurementClientResultAsync(dto)
                : null;
        }

        public async Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return new ClientMeasurementVeexResultDto() { ReturnCode = ReturnCode.NoSuchRtu };

            return rtu.RtuMaker == RtuMaker.VeEX
                ? await _clientToRtuVeexTransmitter.GetClientMeasurementSorBytesAsync(dto)
                : null;
        }

        public async Task<RequestAnswer> PrepareReflectMeasurementAsync(PrepareReflectMeasurementDto dto)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return new RequestAnswer() { ReturnCode = ReturnCode.NoSuchRtu };

            return rtu.RtuMaker == RtuMaker.VeEX
                ? await _clientToRtuVeexTransmitter.PrepareReflectMeasurementAsync(dto)
                : null;
        }

        public async Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            return await _wcfIntermediateC2R.DoOutOfTurnPreciseMeasurementAsync(dto);
        }

        public async Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto)
        {
            return await _wcfIntermediateC2R.InterruptMeasurementAsync(dto);
        }

        public async Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto)
        {
            return await _wcfIntermediateC2R.FreeOtdrAsync(dto);
        }

        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingWebApiProfile>()).CreateMapper();
        public async Task<string> UpdateMeasurement(string username, UpdateMeasurementDto dto)
        {
            var cmd = Mapper.Map<UpdateMeasurement>(dto);
            return await _eventStoreService.SendCommand(cmd, username, dto.ClientIp);
        }

        public async Task<byte[]> GetSorBytes(int sorFileId)
        {
            return await _sorFileRepository.GetSorBytesAsync(sorFileId);
        }

        public async Task<RftsEventsDto> GetRftsEvents(int sorFileId)
        {
            var sorBytes = await _sorFileRepository.GetSorBytesAsync(sorFileId);
            return RftsEventsFactory.Create(sorBytes);
        }
    }
}