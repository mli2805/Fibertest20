using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using AutoMapper;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
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
        private readonly BaseRefsChecker2 _baseRefsChecker;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;
        private readonly IntermediateLayer _intermediateLayer;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        public WcfServiceCommonC2D(GlobalState globalState, IMyLog logFile,
            Model writeModel, SorFileRepository sorFileRepository,
            EventStoreService eventStoreService, ClientsCollection clientsCollection,
            BaseRefsChecker2 baseRefsChecker, BaseRefLandmarksTool baseRefLandmarksTool,
            IntermediateLayer intermediateLayer,
            IFtSignalRClient ftSignalRClient,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter
            )
        {
            _globalState = globalState;
            _logFile = logFile;
            _writeModel = writeModel;
            _sorFileRepository = sorFileRepository;
            _eventStoreService = eventStoreService;
            _clientsCollection = clientsCollection;
            _baseRefsChecker = baseRefsChecker;
            _baseRefLandmarksTool = baseRefLandmarksTool;
            _intermediateLayer = intermediateLayer;
            _ftSignalRClient = ftSignalRClient;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
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

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            return await _clientToRtuTransmitter.CheckRtuConnection(dto);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            return await _intermediateLayer.InitializeRtuAsync(dto);
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            var otauAttachedDto = dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.AttachOtauAsync(dto)
                : new OtauAttachedDto
                {
                    IsAttached = false,
                    ErrorMessage = "This function for VeEX RTU doesn't implemented",
                    ReturnCode = ReturnCode.RtuAttachOtauError
                };
            if (otauAttachedDto.IsAttached)
            {
                AttachOtauIntoGraph(dto, otauAttachedDto);
                await _ftSignalRClient.NotifyAll("FetchTree", null);
            }
            return otauAttachedDto;
        }

        private async void AttachOtauIntoGraph(AttachOtauDto dto, OtauAttachedDto result)
        {
            var cmd = new AttachOtau
            {
                Id = dto.OtauId,
                RtuId = dto.RtuId,
                MasterPort = dto.OpticalPort,
                Serial = result.Serial,
                PortCount = result.PortCount,
                NetAddress = (NetAddress)dto.NetAddress.Clone(),
                IsOk = true,
            };
            var username = _clientsCollection.GetClientByClientIp(dto.ClientIp)?.UserName;
            await _eventStoreService.SendCommand(cmd, username, dto.ClientIp);
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            var otauDetachedDto = await _clientToRtuTransmitter.DetachOtauAsync(dto);
            if (otauDetachedDto.IsDetached)
            {
                RemoveOtauFromGraph(dto);
                await _ftSignalRClient.NotifyAll("FetchTree", null);
            }
            return otauDetachedDto;
        }

        private async void RemoveOtauFromGraph(DetachOtauDto dto)
        {
            var otau = _writeModel.Otaus.FirstOrDefault(o => o.Id == dto.OtauId);
            if (otau == null) return;
            var cmd = new DetachOtau
            {
                Id = dto.OtauId,
                RtuId = dto.RtuId,
                OtauIp = dto.NetAddress.Ip4Address,
                TcpPort = dto.OpticalPort,
                TracesOnOtau = _writeModel.Traces
                    .Where(t => t.OtauPort != null && t.OtauPort.Serial == otau.Serial)
                    .Select(t => t.TraceId)
                    .ToList(),
            };

            var username = _clientsCollection.GetClientByClientIp(dto.ClientIp)?.UserName;
            await _eventStoreService.SendCommand(cmd, username, dto.ClientIp);
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var isStopped = dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.StopMonitoringAsync(dto)
                : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.StopMonitoringAsync(dto).Result);

            if (isStopped)
            {
                var username = _clientsCollection.GetClientByClientIp(dto.ClientIp)?.UserName;
                var cmd = new StopMonitoring { RtuId = dto.RtuId };
                await _eventStoreService.SendCommand(cmd, username, dto.ClientIp);
                await _ftSignalRClient.NotifyAll("MonitoringStopped", cmd.ToCamelCaseJson());
            }

            return isStopped;
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var resultFromRtu = dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.ApplyMonitoringSettingsAsync(dto)
                : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.ApplyMonitoringSettingsAsync(dto).Result);

            if (resultFromRtu.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
            {
                var username = _clientsCollection.GetClientByClientIp(dto.ClientIp)?.UserName;

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
                    return new MonitoringSettingsAppliedDto
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
            return resultFromRtu;
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client from {dto.ClientIp} sent base ref for trace {dto.TraceId.First6()}");
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == dto.TraceId);
            if (trace == null)
                return new BaseRefAssignedDto
                {
                    ErrorMessage = "trace not found",
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed
                };

            //                                   landmarks names will be added
            var checkResult = _baseRefsChecker.AreBaseRefsAcceptable(dto.BaseRefs, trace);
            if (checkResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                return checkResult;

            if (dto.OtauPortDto != null) // trace attached to the real port => send base to RTU
            {
                var transferResult = dto.RtuMaker == RtuMaker.IIT
                    ? await _clientToRtuTransmitter.TransmitBaseRefsToRtu(dto)
                    : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.TransmitBaseRefsToRtu(dto).Result);

                if (transferResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    return transferResult;
            }

            var result = await SaveChangesOnServer(dto);
            if (string.IsNullOrEmpty(result))
                await _ftSignalRClient.NotifyAll("FetchTree", null);

            return !string.IsNullOrEmpty(result)
                ? new BaseRefAssignedDto { ReturnCode = ReturnCode.BaseRefAssignmentFailed }
                : new BaseRefAssignedDto { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };
        }

        private async Task<string> SaveChangesOnServer(AssignBaseRefsDto dto)
        {
            var command = new AssignBaseRef { TraceId = dto.TraceId, BaseRefs = new List<BaseRef>() };
            foreach (var baseRefDto in dto.BaseRefs)
            {
                var oldBaseRef = _writeModel.BaseRefs.FirstOrDefault(b =>
                    b.TraceId == dto.TraceId && b.BaseRefType == baseRefDto.BaseRefType);
                if (oldBaseRef != null)
                    await _sorFileRepository.RemoveSorBytesAsync(oldBaseRef.SorFileId);

                var sorFileId = 0;
                if (baseRefDto.Id != Guid.Empty)
                    sorFileId = await _sorFileRepository.AddSorBytesAsync(baseRefDto.SorBytes);

                var baseRef = new BaseRef
                {
                    TraceId = dto.TraceId,

                    Id = baseRefDto.Id,
                    BaseRefType = baseRefDto.BaseRefType,
                    SaveTimestamp = DateTime.Now,
                    Duration = baseRefDto.Duration,
                    UserName = baseRefDto.UserName,

                    SorFileId = sorFileId,
                };
                command.BaseRefs.Add(baseRef);
            }
            return await _eventStoreService.SendCommand(command, dto.Username, dto.ClientIp);
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
                OtdrId = rtu.OtdrId,
                TraceId = cmd.TraceId,
                OtauPortDto = cmd.OtauPortDto,
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

        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        public async Task<RequestAnswer> AttachTraceAndSendBaseRefs(AttachTraceDto dto)
        {
            _logFile.AppendLine("AttachTraceAndSendBaseRefs started");
            var dto1 = await CreateAssignBaseRefsDto(dto);
            if (dto1 == null) return new RequestAnswer() { ReturnCode = ReturnCode.Error };
            if (dto1.BaseRefs.Any())
            {
                var baseRefAssignedDto = dto1.RtuMaker == RtuMaker.IIT
                    ? await _clientToRtuTransmitter.TransmitBaseRefsToRtu(dto1)
                    : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.TransmitBaseRefsToRtu(dto1).Result);

                if (baseRefAssignedDto.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    return new RequestAnswer()
                        { ReturnCode = baseRefAssignedDto.ReturnCode, ErrorMessage = baseRefAssignedDto.ErrorMessage };
            }
          
            var cmd = new AttachTrace() {TraceId = dto.TraceId, OtauPortDto = dto.OtauPortDto};
            var res = await _eventStoreService.SendCommand(cmd, dto.Username, dto.ClientIp);
            return string.IsNullOrEmpty(res)
                ? new RequestAnswer() { ReturnCode = ReturnCode.Ok }
                : new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = res };
        }

        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        // or user explicitly demands to resend base refs to RTU 
        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client from {dto.ClientIp} asked to re-send base ref for trace {dto.TraceId.First6()}");
            var convertedDto = await ConvertToAssignBaseRefsDto(dto);

            if (convertedDto?.BaseRefs == null)
                return new BaseRefAssignedDto { ReturnCode = ReturnCode.DbCannotConvertThisReSendToAssign };
            if (!convertedDto.BaseRefs.Any())
                return new BaseRefAssignedDto { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };

            return dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.TransmitBaseRefsToRtu(convertedDto)
                : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.TransmitBaseRefsToRtu(convertedDto).Result);
        }

        private async Task<AssignBaseRefsDto> ConvertToAssignBaseRefsDto(ReSendBaseRefsDto dto)
        {
            var result = new AssignBaseRefsDto
            {
                ClientIp = dto.ClientIp,
                RtuId = dto.RtuId,
                OtdrId = dto.OtdrId,
                TraceId = dto.TraceId,
                OtauPortDto = dto.OtauPortDto,
                BaseRefs = new List<BaseRefDto>(),
            };

            foreach (var baseRefDto in dto.BaseRefDtos)
            {
                baseRefDto.SorBytes = await _sorFileRepository.GetSorBytesAsync(baseRefDto.SorFileId);
                result.BaseRefs.Add(baseRefDto);
            }

            return result;
        }

        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            return await _clientToRtuTransmitter.DoClientMeasurementAsync(dto);
        }

        public async Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            return await _clientToRtuTransmitter.DoOutOfTurnPreciseMeasurementAsync(dto);
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