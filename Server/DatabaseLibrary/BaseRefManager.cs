using System;
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

        public async Task<BaseRefAssignedDto> AddUpdateOrRemoveBaseRef(AssignBaseRefDto dto)
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

        private static void AddOrUpdateBaseRef(AssignBaseRefDto dto, MySqlContext dbContext, BaseRefDto baseRef)
        {
            var newBaseRef = PrepareNewRecordFromDto(dbContext, dto, baseRef);

            var existingBaseRef =
                dbContext.BaseRefs.FirstOrDefault(b => b.TraceId == dto.TraceId &&
                                                       b.BaseRefType == baseRef.BaseRefType);

            if (existingBaseRef != null)
                dbContext.BaseRefs.Remove(existingBaseRef);
            dbContext.BaseRefs.Add(newBaseRef);
        }

        private static BaseRef PrepareNewRecordFromDto(MySqlContext dbContext, AssignBaseRefDto dto, BaseRefDto baseRef)
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
    }
}
