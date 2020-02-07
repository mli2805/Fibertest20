﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
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
        private readonly SorFileRepository _sorFileRepository;
        private readonly EventStoreService _eventStoreService;
        private readonly ClientsCollection _clientsCollection;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        public WcfServiceCommonC2D(GlobalState globalState, IMyLog logFile, SorFileRepository sorFileRepository, 
            EventStoreService eventStoreService, ClientsCollection clientsCollection,
            BaseRefLandmarksTool baseRefLandmarksTool,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter
            )
        {
            _globalState = globalState;
            _logFile = logFile;
            _sorFileRepository = sorFileRepository;
            _eventStoreService = eventStoreService;
            _clientsCollection = clientsCollection;
            _baseRefLandmarksTool = baseRefLandmarksTool;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
        }

        public void SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            if (_globalState.IsDatacenterInDbOptimizationMode)
                return new ClientRegisteredDto(){ReturnCode = ReturnCode.Error};

            var result = await _clientsCollection.RegisterClientAsync(dto);
            result.StreamIdOriginal = _eventStoreService.StreamIdOriginal;
            result.SnapshotLastEvent = _eventStoreService.LastEventNumberInSnapshot;
            result.SnapshotLastDate = _eventStoreService.LastEventDateInSnapshot;

            var command = new RegisterClientStation() { RegistrationResult = result.ReturnCode };
            await _eventStoreService.SendCommand(command, dto.UserName, dto.ClientIp);

            return result;
        }


        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            return await _clientToRtuTransmitter.CheckRtuConnection(dto);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            return dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.InitializeAsync(dto)
                : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.InitializeAsync(dto).Result);
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            return await _clientToRtuTransmitter.AttachOtauAsync(dto);
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            return await _clientToRtuTransmitter.DetachOtauAsync(dto);
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            return dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.StopMonitoringAsync(dto)
                : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.StopMonitoringAsync(dto).Result);
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var resultFromRtu = dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.ApplyMonitoringSettingsAsync(dto)
                : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.ApplyMonitoringSettingsAsync(dto).Result);

            if (resultFromRtu.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
            {
                var clientStation = _clientsCollection.GetClientStation(dto.ClientId);
                var cmd = new ChangeMonitoringSettings()
                {
                    RtuId = dto.RtuId,
                    PreciseMeas = dto.Timespans.PreciseMeas.GetFrequency(),
                    PreciseSave = dto.Timespans.PreciseSave.GetFrequency(),
                    FastSave = dto.Timespans.FastSave.GetFrequency(),
                    TracesInMonitoringCycle = dto.Ports.Select(p => p.TraceId).ToList(),
                    IsMonitoringOn = dto.IsMonitoringOn,
                };

                var resultFromEventStore = await _eventStoreService.SendCommand(cmd,
                    clientStation.UserName, clientStation.ClientAddress);

                if (!string.IsNullOrEmpty(resultFromEventStore))
                {
                    return new MonitoringSettingsAppliedDto()
                    {
                        ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError,
                        ExceptionMessage = resultFromEventStore
                    };
                }
            }


            return resultFromRtu;
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent base ref for trace {dto.TraceId.First6()}");
            var result = await SaveChangesOnServer(dto);
            if (!string.IsNullOrEmpty(result))
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentFailed };

            if (dto.OtauPortDto == null) // unattached trace
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };

            return dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.TransmitBaseRefsToRtu(dto)
                : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.TransmitBaseRefsToRtu(dto).Result);
        }

        private async Task<string> SaveChangesOnServer(AssignBaseRefsDto dto)
        {
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
            return await _eventStoreService.SendCommand(command, dto.Username, dto.ClientIp);
        }

        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        // or user explicitly demands to resend base refs to RTU 
        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} asked to re-send base ref for trace {dto.TraceId.First6()}");
            var convertedDto = await ConvertToAssignBaseRefsDto(dto);

            if (convertedDto?.BaseRefs == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.DbCannotConvertThisReSendToAssign };
            if (!convertedDto.BaseRefs.Any())
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };

            return dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.TransmitBaseRefsToRtu(convertedDto)
                : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.TransmitBaseRefsToRtu(convertedDto).Result);
        }

        private async Task<AssignBaseRefsDto> ConvertToAssignBaseRefsDto(ReSendBaseRefsDto dto)
        {
            var result = new AssignBaseRefsDto()
            {
                ClientId = dto.ClientId,
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
    }
 
}