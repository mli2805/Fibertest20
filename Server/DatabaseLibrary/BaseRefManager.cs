using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class BaseRefManager
    {
        private readonly IMyLog _logFile;

        public BaseRefManager(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<BaseRefAssignedDto> AddUpdateOrRemoveBaseRef(AssignBaseRefsDto dto)
        {
            var result = new BaseRefAssignedDto();
            try
            {
                var dbContext = new MySqlContext();

                foreach (var baseRef in dto.BaseRefs)
                {
                    if (baseRef.Id == Guid.Empty)
                        RemoveBaseRef(dbContext, dto.TraceId, baseRef);
                    else
                        AddOrUpdateBaseRef(dto, dbContext, baseRef);
                }
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AddOrUpdateBaseRef:" + e.Message);
                result.ReturnCode = ReturnCode.DbError;
                result.ExceptionMessage = e.Message;
                return result;
            }
            _logFile.AppendLine("Base ref(s) saved in Db");
            result.ReturnCode = ReturnCode.BaseRefAssignedSuccessfully;
            return result;
        }

        private static void AddOrUpdateBaseRef(AssignBaseRefsDto dto, MySqlContext dbContext, BaseRefDto baseRef)
        {
            var newBaseRef = PrepareNewRecordFromDto(dbContext, dto, baseRef);

            var existingBaseRef =
                dbContext.BaseRefs.FirstOrDefault(b => b.TraceId == dto.TraceId &&
                                                       b.BaseRefType == baseRef.BaseRefType);

            if (existingBaseRef != null)
                dbContext.BaseRefs.Remove(existingBaseRef);
            dbContext.BaseRefs.Add(newBaseRef);
        }

        private static BaseRef PrepareNewRecordFromDto(MySqlContext dbContext, AssignBaseRefsDto dto, BaseRefDto baseRef)
        {
            var userId = 0;
            var clientStation = dbContext.ClientStations.FirstOrDefault(s => s.ClientGuid == dto.ClientId);
            if (clientStation != null)
                userId = clientStation.UserId;
            var newBaseRef = new BaseRef()
            {
                BaseRefId = baseRef.Id,
                TraceId = dto.TraceId,
                UserId = userId,
                BaseRefType = baseRef.BaseRefType,
                SaveTimestamp = DateTime.Now,
                SorBytes = baseRef.SorBytes,
            };
            return newBaseRef;
        }

        private static void RemoveBaseRef(MySqlContext dbContext, Guid traceId, BaseRefDto baseRef)
        {
            var oldBaseRef =
                dbContext.BaseRefs.FirstOrDefault(
                    b => b.TraceId == traceId && b.BaseRefType == baseRef.BaseRefType);
            if (oldBaseRef != null)
                dbContext.BaseRefs.Remove(oldBaseRef);
        }

        public async Task<AssignBaseRefsDto> ConvertReSendToAssign(ReSendBaseRefsDto dto)
        {
            var result = new AssignBaseRefsDto()
            {
                TraceId = dto.TraceId,
                RtuId = dto.RtuId,
                ClientId = dto.ClientId,
                OtauPortDto = dto.OtauPortDto,
                BaseRefs = await GetTraceBaseRefs(dto.TraceId)
            };
            return result;
        }

        private async Task<List<BaseRefDto>> GetTraceBaseRefs(Guid traceId)
        {
            var result = new List<BaseRefDto>();
            try
            {
                var dbContext = new MySqlContext();
                var list = await dbContext.BaseRefs.Where(b => b.TraceId == traceId).ToListAsync();
                result.AddRange(
                    list.Select(baseRef => new BaseRefDto()
                    {
                        Id = baseRef.BaseRefId,
                        BaseRefType = baseRef.BaseRefType,
                        SorBytes = baseRef.SorBytes,
                    }));
            }

            catch (Exception e)
            {
                _logFile.AppendLine("GetTraceBaseRefs:" + e.Message);
                return null;
            }
            _logFile.AppendLine($"Db extracted {result.Count} base refs");
            return result;

        }
    }
}
