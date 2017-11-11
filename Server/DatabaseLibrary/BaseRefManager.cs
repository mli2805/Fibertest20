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
                    {
                        var oldBaseRef =
                            dbContext.BaseRefs.FirstOrDefault(
                                b => b.TraceId == dto.TraceId && b.BaseRefType == baseRef.BaseRefType);
                        if (oldBaseRef != null)
                            dbContext.BaseRefs.Remove(oldBaseRef);
                    }
                    else
                    {
                        var newBaseRef = new BaseRef()
                        {
                            BaseRefId = baseRef.Id,
                            TraceId = dto.TraceId,
                            BaseRefType = baseRef.BaseRefType,
                            SaveTimestamp = DateTime.Now,
                            SorBytes = baseRef.SorBytes,
                        };

                        var existingBaseRef =
                            dbContext.BaseRefs.FirstOrDefault(b => b.TraceId == dto.TraceId &&
                                                                   b.BaseRefType == baseRef.BaseRefType);

                        if (existingBaseRef != null)
                            dbContext.BaseRefs.Remove(existingBaseRef);
                        dbContext.BaseRefs.Add(newBaseRef);
                    }
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
    }
}
