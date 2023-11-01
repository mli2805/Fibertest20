using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using System.Threading.Tasks;
using Autofac;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.Graph.RtuOccupy;

namespace Iit.Fibertest.DataCenterCore
{
    public class WcfIntermediate
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly EventStoreService _eventStoreService;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuOccupations _rtuOccupations;
        private readonly SorFileRepository _sorFileRepository;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly BaseRefsCheckerOnServer _baseRefsChecker;
        private readonly BaseRefRepairmanIntermediary _baseRefRepairmanIntermediary;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly ManyChangesToBaseRefs _manyChangesToBaseRefs;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        private readonly DoubleAddress _serverDoubleAddress;

        public WcfIntermediate(ILifetimeScope globalScope, IniFile iniFile, IMyLog logFile,
            Model writeModel, EventStoreService eventStoreService,
            ClientsCollection clientsCollection, RtuOccupations rtuOccupations,
            SorFileRepository sorFileRepository, RtuStationsRepository rtuStationsRepository,
            BaseRefsCheckerOnServer baseRefsChecker, BaseRefRepairmanIntermediary baseRefRepairmanIntermediary,
            IFtSignalRClient ftSignalRClient, ManyChangesToBaseRefs manyChangesToBaseRefs,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _writeModel = writeModel;
            _eventStoreService = eventStoreService;
            _clientsCollection = clientsCollection;
            _rtuOccupations = rtuOccupations;
            _sorFileRepository = sorFileRepository;
            _rtuStationsRepository = rtuStationsRepository;
            _baseRefsChecker = baseRefsChecker;
            _baseRefRepairmanIntermediary = baseRefRepairmanIntermediary;
            _ftSignalRClient = ftSignalRClient;
            _manyChangesToBaseRefs = manyChangesToBaseRefs;
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

        public async Task<BaseRefAssignedDto> SynchronizeBaseRefs(InitializeRtuDto dto)
        {
            var commands = new List<object>();
            foreach (var veexTest in _writeModel.VeexTests)
            {
                var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == veexTest.TraceId);
                if (trace != null && trace.RtuId == dto.RtuId)
                    commands.Add(new RemoveVeexTest() { TestId = veexTest.TestId });
            }
            await _eventStoreService.SendCommands(commands, "server", dto.ClientIp);

            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return new BaseRefAssignedDto() { ReturnCode = ReturnCode.Error };

            var list = _writeModel.CreateReSendDtos(rtu, dto.ConnectionId).ToList();
            BaseRefAssignedDto result = null;
            foreach (var reSendBaseRefsDto in list)
            {
                result = await ReSendBaseRefAsync(reSendBaseRefsDto);
                if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    return result;
            }

            return result;
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            var clientStation = _clientsCollection.Get(dto.ConnectionId);
            _logFile.AppendLine($"Client {clientStation} sent base ref for trace {dto.TraceId.First6()}");
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.AssignBaseRefs, clientStation?.UserName,
                    out RtuOccupationState _))
            {
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.RtuIsBusy };
            }
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == dto.TraceId);
            if (trace == null)
                return new BaseRefAssignedDto
                {
                    ErrorMessage = "trace not found",
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed
                };

            var checkResult = _baseRefsChecker.AreBaseRefsAcceptable(dto.BaseRefs, trace);
            if (checkResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                return checkResult;

            BaseRefAssignedDto transferResult = null;
            if (dto.OtauPortDto != null) // trace attached to the real port => send base to RTU
            {
                transferResult = dto.RtuMaker == RtuMaker.IIT
                    ? await _clientToRtuTransmitter.TransmitBaseRefsToRtuAsync(dto)
                    : await _clientToRtuVeexTransmitter.TransmitBaseRefsToRtuAsync(dto);

                if (transferResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    return transferResult;

                if (dto.RtuMaker == RtuMaker.VeEX)
                // Veex and there are base refs so veexTests table should be updated
                {
                    var updateResult = await _baseRefRepairmanIntermediary.UpdateVeexTestList(transferResult, dto.Username, dto.ClientIp);
                    if (updateResult.ReturnCode != ReturnCode.Ok)
                        return new BaseRefAssignedDto()
                        { ReturnCode = updateResult.ReturnCode, ErrorMessage = updateResult.ErrorMessage };
                }
            }

            var result = await SaveChangesOnServer(dto);
            if (string.IsNullOrEmpty(result))
                await _ftSignalRClient.NotifyAll("FetchTree", null);

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, clientStation?.UserName, out RtuOccupationState _);

            return !string.IsNullOrEmpty(result)
                ? new BaseRefAssignedDto
                {
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                    ErrorMessage = result
                }
                : transferResult ?? new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };
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

        public async Task<CorrectionProgressDto> StartLandmarksCorrection(LandmarksCorrectionDto changesList)
        {
            return await _manyChangesToBaseRefs.StartLandmarksCorrection(changesList);
        }
        public async Task<CorrectionProgressDto> GetLandmarksCorrectionProgress(Guid batchId)
        {
            return await _manyChangesToBaseRefs.GetLandmarksCorrectionProgress(batchId);
        }

        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} asked to re-send base ref for trace {dto.TraceId.First6()}");
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.MeasurementClient, username, out RtuOccupationState _))
            {
                return new BaseRefAssignedDto()
                {
                    ReturnCode = ReturnCode.RtuIsBusy,
                };
            }

            var convertedDto = await ConvertToAssignBaseRefsDto(dto);

            if (convertedDto?.BaseRefs == null)
                return new BaseRefAssignedDto { ReturnCode = ReturnCode.DbCannotConvertThisReSendToAssign };
            if (!convertedDto.BaseRefs.Any())
                return new BaseRefAssignedDto { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };

            var transferResult = dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.TransmitBaseRefsToRtuAsync(convertedDto)
                : await _clientToRtuVeexTransmitter.TransmitBaseRefsToRtuAsync(convertedDto);

            if (transferResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                return transferResult;

            if (dto.RtuMaker == RtuMaker.VeEX)
            // Veex and there are base refs so veexTests table should be updated
            {
                var updateResult = await _baseRefRepairmanIntermediary.UpdateVeexTestList(transferResult, dto.Username, dto.ClientIp);
                if (updateResult.ReturnCode != ReturnCode.Ok)
                    return new BaseRefAssignedDto()
                    { ReturnCode = updateResult.ReturnCode, ErrorMessage = updateResult.ErrorMessage };
            }

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, username, out RtuOccupationState _);

            return transferResult;
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
                MainOtauPortDto = dto.MainOtauPortDto,
                BaseRefs = new List<BaseRefDto>(),
            };

            foreach (var baseRefDto in dto.BaseRefDtos)
            {
                baseRefDto.SorBytes = await _sorFileRepository.GetSorBytesAsync(baseRefDto.SorFileId);
                result.BaseRefs.Add(baseRefDto);
            }

            return result;
        }
    }
}
