using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class GraphPostProcessingRepository
    {
        private readonly IMyLog _logFile;

        public GraphPostProcessingRepository(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<string> ProcessTraceRemoved(Guid traceId)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    var baseRefsList = await dbContext.BaseRefs.Where(m => m.TraceId == traceId).ToListAsync();
                    dbContext.BaseRefs.RemoveRange(baseRefsList);

                    var measurementsList = await dbContext.Measurements.Where(m => m.TraceId == traceId).ToListAsync();
                    var sorIds = measurementsList.Select(m => m.SorFileId).ToList();
                    var sorFilesList = await dbContext.SorFiles.Where(s => sorIds.Contains(s.Id)).ToListAsync();
                    dbContext.SorFiles.RemoveRange(sorFilesList);
                    dbContext.Measurements.RemoveRange(measurementsList);

                    await dbContext.SaveChangesAsync();
                    return null;
                }
            }
            catch (Exception e)
            {
                var message = $"ProcessTraceRemoved: {e.Message}";
                _logFile.AppendLine(message);
                return message;
            }
        }
    }
}
