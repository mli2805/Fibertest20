﻿using System;
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

        private readonly ClientRegistrationManager _clientRegistrationManager;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly BaseRefManager _baseRefManager;
        private readonly MeasurementsRepository _measurementsRepository;
        private readonly NetworkEventsRepository _networkEventsRepository;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public WcfServiceForClient(IMyLog logFile, EventStoreService eventStoreService,
            ClientRegistrationManager clientRegistrationManager, ClientToRtuTransmitter clientToRtuTransmitter,
            RtuStationsRepository rtuStationsRepository, BaseRefManager baseRefManager, 
            MeasurementsRepository measurementsRepository, NetworkEventsRepository networkEventsRepository)
        {
            _logFile = logFile;
            _eventStoreService = eventStoreService;
            _clientRegistrationManager = clientRegistrationManager;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _rtuStationsRepository = rtuStationsRepository;
            _baseRefManager = baseRefManager;
            _measurementsRepository = measurementsRepository;
            _networkEventsRepository = networkEventsRepository;
        }

        public async Task<string> SendCommandAsObj(object cmd)
        {
            // during the tests "client" invokes not the C2DWcfManager's method to communicate by network
            // but right server's method from WcfServiceForClient
            var username = "NCrunch";
            var clientIp = "127.0.0.1";
            return await _eventStoreService.SendCommand(cmd, username, clientIp);
        }

        public async Task<string> SendCommand(string json, string username, string clientIp)
        {
            var cmd = JsonConvert.DeserializeObject(json, JsonSerializerSettings);

            var resultInGraph = await _eventStoreService.SendCommand(cmd, username, clientIp);
            if (resultInGraph != null)
                return resultInGraph;

            // A few commands need postprocessing in Db or RTU
            var removeRtu = cmd as RemoveRtu;
            if (removeRtu != null)
                return await _rtuStationsRepository.RemoveRtuAsync(removeRtu.Id);

            // var attachTrace = cmd as AttachTrace;
            // if trace has base refs they should be sent to RTU
            // if (attachTrace != null)
            //    
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

        public async Task<NetworkEventsList> GetNetworkEvents(int revision)
        {
            return await _networkEventsRepository.GetNetworkEventsAsync(revision);
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

        public Task<byte[]> GetSorBytesOfMeasurement(int sorFileId)
        {
            return _measurementsRepository.GetSorBytesOfMeasurementAsync(sorFileId);
        }

        public async Task<byte[]> GetSorBytesOfLastTraceMeasurement(Guid traceId)
        {
            return await _measurementsRepository.GetSorBytesOfLastTraceMeasurementAsync(traceId);
        }

        public async Task<Measurement> GetLastMeasurementForTrace(Guid traceId)
        {
            return await _measurementsRepository.GetLastMeasurementForTraceAsync(traceId);
        }

        public async Task<MeasurementUpdatedDto> SaveMeasurementChanges(UpdateMeasurementDto dto)
        {
            return await _measurementsRepository.SaveMeasurementChangesAsync(dto);
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            return await _clientRegistrationManager.RegisterClientAsync(dto);
        }

        public async Task UnregisterClientAsync(UnRegisterClientDto dto)
        {
            await _clientRegistrationManager.UnregisterClientAsync(dto);
            _logFile.AppendLine($"Client {dto.ClientId.First6()} exited");
        }

        public Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} checked server connection");
            return Task.FromResult(true);
        }

        public  Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} check rtu {dto.NetAddress.ToStringA()} connection");
            return Task.FromResult(_clientToRtuTransmitter.CheckRtuConnection(dto));
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent initialize rtu {dto.RtuId.First6()} request");
            var result = await _clientToRtuTransmitter.InitializeAsync(dto);
            var message = result.IsInitialized
                ? "RTU initialized successfully, monitoring mode is " + (result.IsMonitoringOn ? "AUTO" : "MANUAL")
                : "RTU initialization failed";
            _logFile.AppendLine(message);
            return result;
        }


        public async Task<bool> StartMonitoringAsync(StartMonitoringDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent start monitoring on rtu {dto.RtuId.First6()} request");
            var result = await _clientToRtuTransmitter.StartMonitoringAsync(dto);
            _logFile.AppendLine($"Start monitoring result is {result}");
            return result;
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent stop monitoring on rtu {dto.RtuId.First6()} request");
            var result = await _clientToRtuTransmitter.StopMonitoringAsync(dto);
            _logFile.AppendLine($"Stop monitoring result is {result}");
            return result;
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent monitoring settings for rtu {dto.RtuId.First6()}");
            var result = await _clientToRtuTransmitter.ApplyMonitoringSettingsAsync(dto);
            _logFile.AppendLine($"Apply monitoring settings result is {result.ReturnCode}");
            return result;
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent base ref for trace {dto.TraceId.First6()}");
            var result = await _baseRefManager.AddUpdateOrRemoveBaseRef(dto);
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                return result;

            if (dto.OtauPortDto == null) // unattached trace
                return result;

            return await _clientToRtuTransmitter.AssignBaseRefAsync(dto);
        }

        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} asked to re-send base ref for trace {dto.TraceId.First6()}");

            var convertedDto = await _baseRefManager.ConvertReSendToAssign(dto);

            if (convertedDto?.BaseRefs == null)
                return new BaseRefAssignedDto() {ReturnCode = ReturnCode.DbCannotConvertThisReSendToAssign};
            if (!convertedDto.BaseRefs.Any())
                return new BaseRefAssignedDto() {ReturnCode = ReturnCode.BaseRefAssignedSuccessfully};

            return await _clientToRtuTransmitter.AssignBaseRefAsync(convertedDto);
        }
    }
}
