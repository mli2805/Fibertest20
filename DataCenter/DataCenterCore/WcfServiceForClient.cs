using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForClient : IWcfServiceForClient
    {
        private readonly EventStoreService _eventStoreService;
        private readonly MeasurementFactory _measurementFactory;

        private readonly IMyLog _logFile;

        private readonly ClientStationsRepository _clientStationsRepository;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly SorFileRepository _sorFileRepository;
        private readonly BaseRefRepairmanIntermediary _baseRefRepairmanIntermediary;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;
        private readonly Smtp _smtp;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public WcfServiceForClient(IMyLog logFile, EventStoreService eventStoreService, MeasurementFactory measurementFactory,
            ClientStationsRepository clientStationsRepository, ClientToRtuTransmitter clientToRtuTransmitter,
            RtuStationsRepository rtuStationsRepository, BaseRefRepairmanIntermediary baseRefRepairmanIntermediary,
            BaseRefLandmarksTool baseRefLandmarksTool, SorFileRepository sorFileRepository, Smtp smtp)
        {
            _logFile = logFile;
            _eventStoreService = eventStoreService;
            _measurementFactory = measurementFactory;
            _clientStationsRepository = clientStationsRepository;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _rtuStationsRepository = rtuStationsRepository;
            _sorFileRepository = sorFileRepository;
            _baseRefRepairmanIntermediary = baseRefRepairmanIntermediary;
            _baseRefLandmarksTool = baseRefLandmarksTool;
            _smtp = smtp;
        }

        public void SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            // tests need this function exists 
        }

        public async Task<int> SendCommands(List<string> jsons, string username, string clientIp) // especially for Migrator.exe
        {
            var cmds = jsons.Select(json => JsonConvert.DeserializeObject(json, JsonSerializerSettings)).ToList();

            await _eventStoreService.SendCommands(cmds, username, clientIp);
            return jsons.Count;
        }

        public async Task<int> SendMeas(List<AddMeasurementFromOldBase> list)
        {
            foreach (var dto in list)
            {
                var sorId = await _sorFileRepository.AddSorBytesAsync(dto.SorBytes);
                if (sorId == -1) return -1;

                var command = _measurementFactory.CreateCommand(dto, sorId);
                await _eventStoreService.SendCommand(command, "migrator", "OnServer");
            }

            return 0;
        }

        public async Task<int> SendCommandsAsObjs(List<object> cmds)
        {
            // during the tests "client" invokes not the C2DWcfManager's method to communicate by network
            // but right server's method from WcfServiceForClient
            var username = "NCrunch";
            var clientIp = "127.0.0.1"; var list = new List<string>();

            foreach (var cmd in cmds)
            {
                list.Add(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings));
            }

            return await SendCommands(list, username, clientIp);
        }

        public async Task<string> SendCommandAsObj(object cmd)
        {
            // during the tests "client" invokes not the C2DWcfManager's method to communicate by network
            // but right server's method from WcfServiceForClient
            var username = "NCrunch";
            var clientIp = "127.0.0.1";
            return await SendCommand(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings), username, clientIp);
        }

        public async Task<string> SendCommand(string json, string username, string clientIp)
        {
            var cmd = JsonConvert.DeserializeObject(json, JsonSerializerSettings);

            var resultInGraph = await _eventStoreService.SendCommand(cmd, username, clientIp);
            if (resultInGraph != null)
                return resultInGraph;

            // A few commands need post-processing in Db or RTU
            if (cmd is RemoveRtu removeRtu)
                return await _rtuStationsRepository.RemoveRtuAsync(removeRtu.RtuId);

            #region Base ref amend
            if (cmd is UpdateAndMoveNode updateAndMoveNode)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(updateAndMoveNode.NodeId);
            if (cmd is UpdateNode updateNode)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(updateNode.NodeId);
            if (cmd is MoveNode moveNode)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(moveNode.NodeId);
            if (cmd is UpdateEquipment updateEquipment)
                return await _baseRefRepairmanIntermediary.ProcessUpdateEquipment(updateEquipment.EquipmentId);
            if (cmd is UpdateFiber updateFiber)
                return await _baseRefRepairmanIntermediary.ProcessUpdateFiber(updateFiber.Id);
            if (cmd is AddNodeIntoFiber addNodeIntoFiber)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(addNodeIntoFiber.Id);
            if (cmd is RemoveNode removeNode && removeNode.Type == EquipmentType.AdjustmentPoint)
                return await _baseRefRepairmanIntermediary.ProcessNodeRemoved(removeNode.DetoursForGraph.Select(d => d.TraceId).ToList());
            #endregion

            return null;
        }

        public async Task<string[]> GetEvents(int revision)
        {
            return await Task.FromResult(_eventStoreService.GetEvents(revision));
        }

        public async Task<byte[]> GetSorBytes(int sorFileId)
        {
            return await _sorFileRepository.GetSorBytesAsync(sorFileId);
        }


        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var result = await _clientStationsRepository.RegisterClientAsync(dto);
            result.GraphDbVersionId = _eventStoreService.GraphDbVersionId;

            if (!dto.IsHeartbeat)
            {
                var command = new RegisterClientStation(){RegistrationResult = result.ReturnCode};
                await _eventStoreService.SendCommand(command, dto.Username, dto.ClientIp);
            }

            return result;
        }

        public async Task<int> UnregisterClientAsync(UnRegisterClientDto dto)
        {
            var result = await _clientStationsRepository.UnregisterClientAsync(dto);
            _logFile.AppendLine($"Client {dto.Username} from {dto.ClientIp} exited");

            var command = new UnregisterClientStation();
            await _eventStoreService.SendCommand(command, dto.Username, dto.ClientIp);

            return result;
        }

        public async Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} checked server connection");
            await Task.Delay(1);
            return true;
        }

        public async Task<bool> SendTestEmail()
        {
            _logFile.AppendLine("Client asked test dispatch");
            return await _smtp.SendTestDispatch();
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} check RTU {dto.NetAddress.ToStringA()} connection");
            return await _clientToRtuTransmitter.CheckRtuConnection(dto);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent initialize RTU {dto.RtuId.First6()} request");
            var result = await _clientToRtuTransmitter.InitializeAsync(dto);
            var message = result.IsInitialized
                ? "RTU initialized successfully, monitoring mode is " + (result.IsMonitoringOn ? "AUTO" : "MANUAL")
                : "RTU initialization failed";
            _logFile.AppendLine(message);
            return result;
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent attach OTAU {dto.OtauId.First6()} request");
            var result = await _clientToRtuTransmitter.AttachOtauAsync(dto);
            var message = result.IsAttached ? "OTAU attached successfully" : "Failed to attach OTAU";
            _logFile.AppendLine(message);
            return result;
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent detach OTAU {dto.OtauId.First6()} request");
            var result = await _clientToRtuTransmitter.DetachOtauAsync(dto);
            var message = result.IsDetached ? "OTAU detached successfully" : "Failed to detach OTAU";
            _logFile.AppendLine(message);
            return result;
        }


        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent stop monitoring on RTU {dto.RtuId.First6()} request");
            var result = await _clientToRtuTransmitter.StopMonitoringAsync(dto);
            _logFile.AppendLine($"Stop monitoring result is {result}");
            return result;
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent monitoring settings for RTU {dto.RtuId.First6()}");
            var result = await _clientToRtuTransmitter.ApplyMonitoringSettingsAsync(dto);
            _logFile.AppendLine($"Apply monitoring settings result is {result.ReturnCode}");
            return result;
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent base ref for trace {dto.TraceId.First6()}");
            foreach (var sorFileId in dto.DeleteOldSorFileIds)
            {
                await _sorFileRepository.RemoveSorBytesAsync(sorFileId);
            }
            var command = new AssignBaseRef() { TraceId = dto.TraceId, BaseRefs = new List<BaseRef>() };
            foreach (var baseRefDto in dto.BaseRefs)
            {
                var sorFileId = 0;
                if (baseRefDto.Id != Guid.Empty)
                    sorFileId = await _sorFileRepository.AddSorBytesAsync(baseRefDto.SorBytes);

                var baseRef = new BaseRef()
                {
                    TraceId = dto.TraceId,

                    Id = baseRefDto.Id,
                    BaseRefType = baseRefDto.BaseRefType,
                    SaveTimestamp = baseRefDto.SaveTimestamp,
                    Duration = baseRefDto.Duration,
                    UserName = baseRefDto.UserName,

                    SorFileId = sorFileId,
                };
                command.BaseRefs.Add(baseRef);
            }
            await _eventStoreService.SendCommand(command, dto.Username, dto.ClientIp);

            if (dto.OtauPortDto == null) // unattached trace
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };

            return await _clientToRtuTransmitter.AssignBaseRefAsync(dto);
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsyncFromMigrator(AssignBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent base ref for trace {dto.TraceId.First6()}");
            foreach (var sorFileId in dto.DeleteOldSorFileIds)
            {
                await _sorFileRepository.RemoveSorBytesAsync(sorFileId);
            }
            var command = new AssignBaseRef() { TraceId = dto.TraceId, BaseRefs = new List<BaseRef>() };
            foreach (var baseRefDto in dto.BaseRefs)
            {
                var sorFileId = 0;
                if (baseRefDto.Id != Guid.Empty)
                {
                    if (baseRefDto.BaseRefType == BaseRefType.Fast)
                        _baseRefLandmarksTool.AugmentFastBaseRefSentByMigrator(dto.TraceId, baseRefDto);

                    sorFileId = await _sorFileRepository.AddSorBytesAsync(baseRefDto.SorBytes);
                }

                var baseRef = new BaseRef()
                {
                    TraceId = dto.TraceId,

                    Id = baseRefDto.Id,
                    BaseRefType = baseRefDto.BaseRefType,
                    SaveTimestamp = baseRefDto.SaveTimestamp,
                    Duration = baseRefDto.Duration,
                    UserName = baseRefDto.UserName,

                    SorFileId = sorFileId,
                };
                command.BaseRefs.Add(baseRef);
            }
            await _eventStoreService.SendCommand(command, "migrator", "OnServer");
            return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };
        }

        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        // or user explicitly demands to resend base refs to RTU 
        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} asked to re-send base ref for trace {dto.TraceId.First6()}");

            var convertedDto = await Convert(dto);

            if (convertedDto?.BaseRefs == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.DbCannotConvertThisReSendToAssign };
            if (!convertedDto.BaseRefs.Any())
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };

            return await _clientToRtuTransmitter.AssignBaseRefAsync(convertedDto);
        }

        private async Task<AssignBaseRefsDto> Convert(ReSendBaseRefsDto dto)
        {
            var result = new AssignBaseRefsDto()
            {
                ClientId = dto.ClientId,
                RtuId = dto.RtuId,
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
            _logFile.AppendLine($"Client {dto.ClientId.First6()} asked to do client's measurement on RTU {dto.RtuId.First6()}");
            return await _clientToRtuTransmitter.DoClientMeasurementAsync(dto);
        }

        public async Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} asked to do out of turn measurement on RTU {dto.RtuId.First6()}");
            return await _clientToRtuTransmitter.DoOutOfTurnPreciseMeasurementAsync(dto);
        }
    }
}
