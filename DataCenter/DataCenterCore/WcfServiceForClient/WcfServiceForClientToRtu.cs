using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceForClient
    {
        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            return await _clientToRtuTransmitter.CheckRtuConnection(dto);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            return dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.InitializeAsync(dto)
                : await Task.Factory.StartNew(()=> _clientToRtuVeexTransmitter.InitializeAsync(dto).Result);
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
                : await Task.Factory.StartNew(()=> _clientToRtuVeexTransmitter.StopMonitoringAsync(dto).Result);
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            return dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.ApplyMonitoringSettingsAsync(dto)
                : await Task.Factory.StartNew(()=> _clientToRtuVeexTransmitter.ApplyMonitoringSettingsAsync(dto).Result);
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent base ref for trace {dto.TraceId.First6()}");
            var result = await SaveChangesOnServer(dto);
            if (!string.IsNullOrEmpty(result))
                return new BaseRefAssignedDto() {ReturnCode = ReturnCode.BaseRefAssignmentFailed };

            if (dto.OtauPortDto == null) // unattached trace
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };

            return dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.TransmitBaseRefsToRtu(dto)
                : await Task.Factory.StartNew(()=> _clientToRtuVeexTransmitter.TransmitBaseRefsToRtu(dto).Result);
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
            return await _clientToRtuTransmitter.ReSendBaseRefAsync(dto);
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