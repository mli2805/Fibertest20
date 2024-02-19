using System;
using System.Collections.Generic;
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
        private readonly BaseRefRepairmanIntermediary _baseRefRepairmanIntermediary;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly RtuOccupations _rtuOccupations;
        private readonly WcfIntermediate _wcfIntermediate;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;
        private readonly ClientToLinuxRtuHttpTransmitter _clientToLinuxRtuHttpTransmitter;

        public WcfServiceCommonC2D(GlobalState globalState, IMyLog logFile,
            Model writeModel, SorFileRepository sorFileRepository,
            EventStoreService eventStoreService, ClientsCollection clientsCollection,
            BaseRefLandmarksTool baseRefLandmarksTool,
            BaseRefRepairmanIntermediary baseRefRepairmanIntermediary,
            IFtSignalRClient ftSignalRClient, RtuOccupations rtuOccupations,
            WcfIntermediate wcfIntermediate,
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
            _baseRefRepairmanIntermediary = baseRefRepairmanIntermediary;
            _ftSignalRClient = ftSignalRClient;
            _rtuOccupations = rtuOccupations;
            _wcfIntermediate = wcfIntermediate;
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
            return await _clientToRtuVeexTransmitter.CheckRtuConnection(dto);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            return await _wcfIntermediate.InitializeRtuAsync(dto);
        }

        public async Task<RtuCurrentStateDto> GetRtuCurrentState(GetCurrentRtuStateDto dto)
        {
            var state = await _clientToLinuxRtuHttpTransmitter.GetRtuCurrentState(dto);
            _logFile.AppendLine($"state initialization is null - {state.LastInitializationResult == null}");
            return state;
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.AttachOrDetachOtau, username, out RtuOccupationState _))
                return new OtauAttachedDto()
                {
                    ReturnCode = ReturnCode.RtuIsBusy,
                    IsAttached = false,
                };

            var otauAttachedDto = await _wcfIntermediate.AttachOtauAsync(dto);
            if (otauAttachedDto.IsAttached)
            {
                AttachOtauIntoGraph(dto, otauAttachedDto);
                await _ftSignalRClient.NotifyAll("FetchTree", null);
            }

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, username, out RtuOccupationState _);

            return otauAttachedDto;
        }

        private async void AttachOtauIntoGraph(AttachOtauDto dto, OtauAttachedDto result)
        {
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            var cmd = new AttachOtau
            {
                Id = result.OtauId,
                RtuId = dto.RtuId,
                MasterPort = dto.OpticalPort,
                Serial = result.Serial,
                PortCount = result.PortCount,
                NetAddress = dto.NetAddress.Clone(),
                IsOk = true,
            };
            await _eventStoreService.SendCommand(cmd, username, dto.ClientIp);
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.AttachOrDetachOtau, username, out RtuOccupationState _))
                return new OtauDetachedDto()
                {
                    ReturnCode = ReturnCode.RtuIsBusy,
                    IsDetached = false,
                };

            var otauDetachedDto = await _wcfIntermediate.DetachOtauAsync(dto);

            if (otauDetachedDto.IsDetached)
            {
                var res = await RemoveOtauFromGraph(dto);
                if (string.IsNullOrEmpty(res))
                    await _ftSignalRClient.NotifyAll("FetchTree", null);
            }

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, username, out RtuOccupationState _);

            return otauDetachedDto;
        }

        private async Task<string> RemoveOtauFromGraph(DetachOtauDto dto)
        {
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            var otau = _writeModel.Otaus.FirstOrDefault(o => o.Id == dto.OtauId);
            if (otau == null) return null;
            var cmd = new DetachOtau
            {
                Id = dto.OtauId,
                RtuId = dto.RtuId,
                OtauIp = dto.NetAddress.Ip4Address,
                TcpPort = dto.NetAddress.Port,
                TracesOnOtau = _writeModel.Traces
                    .Where(t => t.OtauPort != null && t.OtauPort.Serial == otau.Serial)
                    .Select(t => t.TraceId)
                    .ToList(),
            };

            return await _eventStoreService.SendCommand(cmd, username, dto.ClientIp);
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.MeasurementClient, username, out RtuOccupationState _))
                return false;

            var stopResult = await _wcfIntermediate.StopMonitoringAsync(dto);
            var isStopped = stopResult.ReturnCode == ReturnCode.Ok;

            if (isStopped)
            {
                var cmd = new StopMonitoring { RtuId = dto.RtuId };
                await _eventStoreService.SendCommand(cmd, username, dto.ClientIp);
                await _ftSignalRClient.NotifyAll("MonitoringStopped", cmd.ToCamelCaseJson());
            }

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, username, out RtuOccupationState _);
            return isStopped;
        }

        public async Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.MonitoringSettings, username, out RtuOccupationState _))
                return new RequestAnswer() { ReturnCode = ReturnCode.RtuIsBusy };

            foreach (var portWithTraceDto in dto.Ports)
            {
                var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == portWithTraceDto.TraceId);
                portWithTraceDto.LastTraceState = trace?.State ?? FiberState.Unknown;
            }

            var resultFromRtu = await _wcfIntermediate.ApplyMonitoringSettingsAsync(dto);

            if (resultFromRtu.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
            {
                var cmd = new ChangeMonitoringSettings
                {
                    RtuId = dto.RtuId,
                    PreciseMeas = dto.Timespans.PreciseMeas.GetFrequency(),
                    PreciseSave = dto.Timespans.PreciseSave.GetFrequency(),
                    FastSave = dto.Timespans.FastSave.GetFrequency(),
                    TracesInMonitoringCycle = dto.Ports.Select(p => p.TraceId).ToList(),
                    IsMonitoringOn = dto.IsMonitoringOn,
                };

                var resultFromEventStore = await _eventStoreService.SendCommand(cmd, username, dto.ClientIp);

                if (!string.IsNullOrEmpty(resultFromEventStore))
                {
                    return new RequestAnswer
                    {
                        ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError,
                        ErrorMessage = resultFromEventStore
                    };
                }
                else
                {
                    if (dto.IsMonitoringOn)
                        await _ftSignalRClient.NotifyAll("MonitoringStarted", $"{{\"rtuId\" : \"{dto.RtuId}\"}}");
                    else
                        await _ftSignalRClient.NotifyAll("MonitoringStopped", $"{{\"rtuId\" : \"{dto.RtuId}\"}}");
                }
            }

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, username, out RtuOccupationState _);
            return resultFromRtu;
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            return await _wcfIntermediate.AssignBaseRefAsync(dto);
        }


        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        public async Task<RequestAnswer> AttachTraceAndSendBaseRefs(AttachTraceDto dto)
        {
            var clientStation = _clientsCollection.Get(dto.ConnectionId);
            var username = clientStation?.UserName;
            _logFile.AppendLine($@"User {clientStation} asked attach trace and re-send base refs");

            var trace = _writeModel.Traces.First(t => t.TraceId == dto.TraceId);
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null)
                return new RequestAnswer() { ReturnCode = ReturnCode.NoSuchRtu };
            if (!_rtuOccupations.TrySetOccupation(rtu.Id, RtuOccupation.AttachTrace, username, out RtuOccupationState _))
                return new RequestAnswer() { ReturnCode = ReturnCode.RtuIsBusy };

            try
            {
                BaseRefAssignedDto transferResult = await SendBaseRefsIfAny(dto);

                if (transferResult != null && transferResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    return new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = transferResult.ErrorMessage };

                var cmd = new AttachTrace() { TraceId = dto.TraceId, OtauPortDto = dto.OtauPortDto };
                var res = await _eventStoreService.SendCommand(cmd, dto.Username, dto.ClientIp);

                if (!string.IsNullOrEmpty(res))
                    return new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = res };

                await NotifyWebClientTraceAttached(dto.TraceId);

                if (transferResult == null || dto.RtuMaker == RtuMaker.IIT)
                    return new RequestAnswer() { ReturnCode = ReturnCode.Ok };

                // Veex and there are base refs so veexTests table should be updated
                return await _baseRefRepairmanIntermediary.UpdateVeexTestList(transferResult, dto.Username, dto.ClientIp);
            }
            finally
            {
                _rtuOccupations.TrySetOccupation(rtu.Id, RtuOccupation.None, username, out RtuOccupationState _);
            }
        }

        public async Task<CorrectionProgressDto> StartLandmarksCorrection(LandmarksCorrectionDto changesList)
        {
            return await _wcfIntermediate.StartLandmarksCorrection(changesList);
        }

        public async Task<CorrectionProgressDto> GetLandmarksCorrectionProgress(Guid batchId)
        {
            return await _wcfIntermediate.GetLandmarksCorrectionProgress(batchId);
        }


        private async Task<BaseRefAssignedDto> SendBaseRefsIfAny(AttachTraceDto dto)
        {
            var dto1 = await CreateAssignBaseRefsDto(dto);
            if (dto1 == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.Error, ErrorMessage = "RTU or trace not found!" };

            if (dto1.BaseRefs.Any())
            {
                return await _baseRefRepairmanIntermediary.TransmitBaseRefs(dto1);
            }
            return null;
        }

        private async Task<AssignBaseRefsDto> CreateAssignBaseRefsDto(AttachTraceDto cmd)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == cmd.TraceId);
            if (trace == null) return null;
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null) return null;

            var dto = new AssignBaseRefsDto()
            {
                RtuId = trace.RtuId,
                RtuMaker = rtu.RtuMaker,
                OtdrId = rtu.OtdrId,
                TraceId = cmd.TraceId,
                OtauPortDto = cmd.OtauPortDto,
                MainOtauPortDto = cmd.MainOtauPortDto,
                BaseRefs = new List<BaseRefDto>(),
            };

            foreach (var baseRef in _writeModel.BaseRefs.Where(b => b.TraceId == trace.TraceId))
            {
                dto.BaseRefs.Add(new BaseRefDto()
                {
                    SorFileId = baseRef.SorFileId,
                    Id = baseRef.TraceId,
                    BaseRefType = baseRef.BaseRefType,
                    Duration = baseRef.Duration,
                    SaveTimestamp = baseRef.SaveTimestamp,
                    UserName = baseRef.UserName,

                    SorBytes = await _sorFileRepository.GetSorBytesAsync(baseRef.SorFileId),
                });
            }

            return dto;
        }

        private async Task NotifyWebClientTraceAttached(Guid traceId)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace != null)
            {
                var meas = _writeModel.Measurements.LastOrDefault(m => m.TraceId == traceId);
                var signal = new TraceTachDto()
                {
                    TraceId = traceId,
                    Attach = true,
                    TraceState = trace.State,
                    SorFileId = meas?.SorFileId ?? -1
                };

                await _ftSignalRClient.NotifyAll("TraceTach", signal.ToCamelCaseJson());
            }
        }

        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        // or user explicitly demands to resend base refs to RTU 
        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            return await _wcfIntermediate.ReSendBaseRefAsync(dto);
        }

        public async Task<ClientMeasurementStartedDto> StartClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            _logFile.AppendLine($"Client {username} asked to do measurement on RTU {dto.RtuId.First6()}");
            var occupation = dto.IsForAutoBase ? RtuOccupation.AutoBaseMeasurement : RtuOccupation.MeasurementClient;
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, occupation, username, out RtuOccupationState currentState))
            {
                return new ClientMeasurementStartedDto()
                {
                    ReturnCode = ReturnCode.RtuIsBusy,
                    RtuOccupationState = currentState,
                };
            }

            return await _wcfIntermediate.DoClientMeasurementAsync(dto);

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
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.PreciseMeasurementOutOfTurn, username, out RtuOccupationState currentState))
            {
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.RtuIsBusy,
                    RtuOccupationState = currentState,
                };
            }

            var result = await _wcfIntermediate.DoOutOfTurnPreciseMeasurementAsync(dto);

            return result;
        }

        public async Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return new RequestAnswer() { ReturnCode = ReturnCode.NoSuchRtu };

            return rtu.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.InterruptMeasurementAsync(dto)
                : await _clientToRtuVeexTransmitter.InterruptMeasurementAsync(dto);
        }

        public async Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return new RequestAnswer() { ReturnCode = ReturnCode.NoSuchRtu };

            return rtu.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.FreeOtdrAsync(dto)
                : await _clientToRtuVeexTransmitter.FreeOtdrAsync(dto);
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

        public async Task<RtuCurrentStateDto> SayHello(string name)
        {
            await Task.Delay(0);
            return new RtuCurrentStateDto(ReturnCode.Ok);
        }
    }

}