using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
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
                transferResult = await _baseRefRepairmanIntermediary.TransmitBaseRefs(dto);

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

    }
}
