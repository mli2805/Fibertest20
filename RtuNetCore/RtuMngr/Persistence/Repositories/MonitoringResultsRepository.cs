using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public class MonitoringResultsRepository(RtuContext rtuContext, ILogger<MonitoringResultsRepository> logger)
{
    public async Task Add(MonitoringResultEf entity)
    {
        logger.Info(Logs.RtuManager, $"persist monitoring result with state {entity.TraceState} for trace {entity.TraceId}");
        await rtuContext.MonitoringResults.AddAsync(entity);
        await rtuContext.SaveChangesAsync();
    }

    public async Task<List<MonitoringResultDto>> GetPortionYoungerThan(DateTime lastReceived)
    {
        const int portionSize = 3;

        try
        {
            // remove older
            var olderThan = await rtuContext.MonitoringResults
                .OrderBy(r => r.TimeStamp)
                .Where(r => r.TimeStamp <= lastReceived.AddSeconds(1))
                .ToListAsync();

            rtuContext.MonitoringResults.RemoveRange(olderThan);
            await rtuContext.SaveChangesAsync();

            // get portion
            var portionEf = await rtuContext.MonitoringResults
                .OrderBy(r => r.TimeStamp)
                .Take(portionSize)
                .ToListAsync();

            foreach (var monitoringResultEf in portionEf)
            {
                logger.Info(Logs.RtuService, $"Made at {monitoringResultEf.TimeStamp} for trace {monitoringResultEf.TraceId} state is {monitoringResultEf.TraceState.ToString()}");
            }

            return portionEf.Select(r => r.FromEf()).ToList();
        }
        catch (Exception e)
        {
            logger.Exception(Logs.RtuService, e,  "GetPortionYoungerThan");
            return new List<MonitoringResultDto>();
        }
    }
}