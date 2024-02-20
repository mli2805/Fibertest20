﻿using Iit.Fibertest.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
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

            var transferResult = await _baseRefRepairmanIntermediary.TransmitBaseRefs(convertedDto);

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
