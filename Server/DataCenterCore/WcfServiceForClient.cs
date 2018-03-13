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

        private readonly IMyLog _logFile;

        private readonly ClientStationsRepository _clientStationsRepository;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly MeasurementsRepository _measurementsRepository;
        private readonly NetworkEventsRepository _networkEventsRepository;
        private readonly BopNetworkEventsRepository _bopNetworkEventsRepository;
        private readonly GraphPostProcessingRepository _graphPostProcessingRepository;
        private readonly BaseRefsRepositoryIntermediary _baseRefsRepositoryIntermediary;
        private readonly BaseRefRepairmanIntermediary _baseRefRepairmanIntermediary;
        private readonly MeasurementChangerIntermediary _measurementChangerIntermediary;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public WcfServiceForClient(IMyLog logFile, EventStoreService eventStoreService, 
            ClientStationsRepository clientStationsRepository, ClientToRtuTransmitter clientToRtuTransmitter,
            RtuStationsRepository rtuStationsRepository, MeasurementChangerIntermediary measurementChangerIntermediary,
            BaseRefsRepositoryIntermediary baseRefsRepositoryIntermediary, BaseRefRepairmanIntermediary baseRefRepairmanIntermediary,
            MeasurementsRepository measurementsRepository, NetworkEventsRepository networkEventsRepository,
            BopNetworkEventsRepository bopNetworkEventsRepository, GraphPostProcessingRepository graphPostProcessingRepository)
        {
            _logFile = logFile;
            _eventStoreService = eventStoreService;
            _clientStationsRepository = clientStationsRepository;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _rtuStationsRepository = rtuStationsRepository;
            _measurementsRepository = measurementsRepository;
            _networkEventsRepository = networkEventsRepository;
            _bopNetworkEventsRepository = bopNetworkEventsRepository;
            _graphPostProcessingRepository = graphPostProcessingRepository;
            _baseRefsRepositoryIntermediary = baseRefsRepositoryIntermediary;
            _baseRefRepairmanIntermediary = baseRefRepairmanIntermediary;
            _measurementChangerIntermediary = measurementChangerIntermediary;
        }

        public async Task<int> SendCommands(List<string> jsons, string username, string clientIp) // especially for Migrator.exe
        {
            var cmds = jsons.Select(json => JsonConvert.DeserializeObject(json, JsonSerializerSettings)).ToList();

            await _eventStoreService.SendCommands(cmds, username, clientIp);
            return jsons.Count;
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
                return await _rtuStationsRepository.RemoveRtuAsync(removeRtu.Id);

            if (cmd is RemoveTrace removeTrace)
                return await _graphPostProcessingRepository.ProcessTraceRemovedAsync(removeTrace.Id);

            #region Base ref amend
            if (cmd is MoveNode moveNode)
                return await _baseRefRepairmanIntermediary.ProcessNodeMoved(moveNode.NodeId);
            if (cmd is UpdateEquipment updateEquipment)
                return await _baseRefRepairmanIntermediary.ProcessUpdateEquipment(updateEquipment.Id);
            if (cmd is UpdateFiber updateFiber)
                return await _baseRefRepairmanIntermediary.ProcessUpdateFiber(updateFiber.Id);
            if (cmd is AddNodeIntoFiber addNodeIntoFiber)
                return await _baseRefRepairmanIntermediary.ProcessAddIntoFiber(addNodeIntoFiber.Id);
            if (cmd is RemoveNode removeNode && removeNode.Type == EquipmentType.AdjustmentPoint)
                return await _baseRefRepairmanIntermediary.ProcessNodeRemoved(removeNode.TraceWithNewFiberForDetourRemovedNode.Keys.ToList());
            #endregion

            return null;
        }

        public async Task<string[]> GetEvents(int revision)
        {
            return await Task.FromResult(_eventStoreService.GetEvents(revision));
        }

        public async Task<MeasurementsList> GetOpticalEvents()
        {
            return await _measurementsRepository.GetOpticalEventsAsync();
        }

        public async Task<NetworkEventsList> GetNetworkEvents()
        {
            return await _networkEventsRepository.GetNetworkEventsAsync();
        }

        public async Task<BopNetworkEventsList> GetBopNetworkEvents()
        {
            return await _bopNetworkEventsRepository.GetBopNetworkEventsAsync();
        }

        public async Task<TraceStatistics> GetTraceStatistics(Guid traceId)
        {
            var traceStatistics = await _measurementsRepository.GetTraceMeasurementsAsync(traceId);
            _logFile.AppendLine($"There {traceStatistics.BaseRefs.Count} base refs and {traceStatistics.Measurements.Count} measurements");
            return traceStatistics;
        }

        public Task<byte[]> GetSorBytesOfBase(Guid baseRefId)
        {
            return _measurementsRepository.GetSorBytesOfBaseAsync(baseRefId);
        }

        public Task<byte[]> GetSorBytes(int sorFileId)
        {
            return _measurementsRepository.GetSorBytesAsync(sorFileId);
        }

        public async Task<byte[]> GetSorBytesOfLastTraceMeasurement(Guid traceId)
        {
            return await _measurementsRepository.GetSorBytesOfLastTraceMeasurementAsync(traceId);
        }

        public async Task<MeasurementWithSor> GetLastMeasurementForTrace(Guid traceId)
        {
            return await _measurementsRepository.GetLastMeasurementForTraceAsync(traceId);
        }

        public async Task<MeasurementUpdatedDto> SaveMeasurementChanges(UpdateMeasurementDto dto)
        {
            return await _measurementChangerIntermediary.UpdateMeasurementAsync(dto);
        }

        public async Task<List<BaseRefDto>> GetTraceBaseRefsAsync(Guid traceId)
        {
            return await _baseRefsRepositoryIntermediary.GetTraceBaseRefsAsync(traceId);
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var result = await _clientStationsRepository.RegisterClientAsync(dto);
            result.GraphDbVersionId = _eventStoreService.GraphDbVersionId;
            return result;
        }

        public async Task UnregisterClientAsync(UnRegisterClientDto dto)
        {
            await _clientStationsRepository.UnregisterClientAsync(dto);
            _logFile.AppendLine($"Client {dto.ClientId.First6()} exited");
        }

        public Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} checked server connection");
            return Task.FromResult(true);
        }

        public  async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
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

        public  async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
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
            var result = await _baseRefsRepositoryIntermediary.AssignBaseRefAsync(dto);
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                return result;

            if (dto.OtauPortDto == null) // unattached trace
                return result;

            return await _clientToRtuTransmitter.AssignBaseRefAsync(dto);
        }

        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        // or user explicitly demands to resend base refs to RTU 
        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} asked to re-send base ref for trace {dto.TraceId.First6()}");

            var convertedDto = await _baseRefsRepositoryIntermediary.ConvertReSendToAssign(dto);

            if (convertedDto?.BaseRefs == null)
                return new BaseRefAssignedDto() {ReturnCode = ReturnCode.DbCannotConvertThisReSendToAssign};
            if (!convertedDto.BaseRefs.Any())
                return new BaseRefAssignedDto() {ReturnCode = ReturnCode.BaseRefAssignedSuccessfully};

            return await _clientToRtuTransmitter.AssignBaseRefAsync(convertedDto);
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
