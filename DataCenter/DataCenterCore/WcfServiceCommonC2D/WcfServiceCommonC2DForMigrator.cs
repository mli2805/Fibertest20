using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceCommonC2D
    {
        public async Task<BaseRefAssignedDto> AssignBaseRefAsyncFromMigrator(AssignBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client from {dto.ClientIp} sent base ref for trace {dto.TraceId.First6()}");
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
                    {
                        try
                        {
                            _baseRefLandmarksTool.AugmentFastBaseRefSentByMigrator(dto.TraceId, baseRefDto);

                        }
                        catch (Exception e)
                        {
                            _logFile.AppendLine("AugmentFastBaseRefSentByMigrator: " + e.Message);
                            return new BaseRefAssignedDto()
                            {
                                ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                                ErrorMessage = "AugmentFastBaseRefSentByMigrator " + e.Message,
                            };
                        }
                    }

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
    }
}
