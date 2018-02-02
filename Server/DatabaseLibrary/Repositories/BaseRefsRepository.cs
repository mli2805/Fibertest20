using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{

    public class BaseRefsRepository
    {
        private readonly ISettings _settings;
        private readonly IMyLog _logFile;

        public BaseRefsRepository(ISettings settings, IMyLog logFile)
        {
            _settings = settings;
            _logFile = logFile;
        }

        public async Task<BaseRefAssignedDto> AddUpdateOrRemoveBaseRef(List<BaseRef> baseRefs)
        {
            var result = new BaseRefAssignedDto();
            try
            {
                using (var dbContext = new FtDbContext(_settings.MySqlConString))
                {
                    foreach (var baseRef in baseRefs)
                    {
                        // in any case - if exists base with the same type for this trace - it should be removed
                        var existingBaseRef = dbContext.BaseRefs.FirstOrDefault
                            (b => b.TraceId == baseRef.TraceId && b.BaseRefType == baseRef.BaseRefType);
                        if (existingBaseRef != null)
                            dbContext.BaseRefs.Remove(existingBaseRef);

                        // if user sent replacement (not empty base) - it should be saved 
                        if (baseRef.BaseRefId != Guid.Empty)
                            dbContext.BaseRefs.Add(baseRef);
                    }
                    await dbContext.SaveChangesAsync();
                    _logFile.AppendLine("Base ref(s) saved in Db");
                    result.ReturnCode = ReturnCode.BaseRefAssignedSuccessfully;
                    return result;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AddUpdateOrRemoveBaseRef:" + e.Message);
                result.ReturnCode = ReturnCode.DbError;
                result.ExceptionMessage = e.Message;
                return result;
            }
        }
    

        public async Task<List<BaseRefDto>> GetTraceBaseRefsAsync(Guid traceId)
        {
            var result = new List<BaseRefDto>();
            try
            {
                using (var dbContext = new FtDbContext(_settings.MySqlConString))
                {
                _logFile.AppendLine("GetTraceBaseRefsAsync: db connected");
                    var list = await dbContext.BaseRefs.Where(b => b.TraceId == traceId).ToListAsync();
                _logFile.AppendLine("GetTraceBaseRefsAsync: list retrieved");
                    result.AddRange(
                        list.Select(baseRef => new BaseRefDto()
                        {
                            Id = baseRef.BaseRefId,
                            BaseRefType = baseRef.BaseRefType,
                            UserName = baseRef.UserName,
                            SaveTimestamp = baseRef.SaveTimestamp,
                            SorBytes = baseRef.SorBytes,
                        }));
                }
            }

            catch (Exception e)
            {
                _logFile.AppendLine("GetTraceBaseRefsAsync:" + e.Message);
                return null;
            }
            _logFile.AppendLine($"Db extracted {result.Count} base refs");
            return result;
        }
    }
}
