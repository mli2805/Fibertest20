using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public class MonitoringResultsRepository
{
    private readonly RtuContext _rtuContext;
    private readonly ILogger<MonitoringResultsRepository> _logger;

    public MonitoringResultsRepository(RtuContext rtuContext, ILogger<MonitoringResultsRepository> logger)
    {
        _rtuContext = rtuContext;
        _logger = logger;
    }

    public async Task Add(MonitoringResultEf entity)
    {
        _logger.Info(Logs.RtuManager, $"persist monitoring result with state {entity.TraceState} for trace {entity.TraceId}");
        await _rtuContext.MonitoringResults.AddAsync(entity);
        await _rtuContext.SaveChangesAsync();
    }

    public async Task<List<MonitoringResultDto>> GetPortionYoungerThan(DateTime lastReceived)
    {
        const int portionSize = 3;

        try
        {
            // remove older
            var olderThan = await _rtuContext.MonitoringResults
                .OrderBy(r => r.TimeStamp)
                .Where(r => r.TimeStamp <= lastReceived.AddSeconds(1))
                .ToListAsync();

            _rtuContext.MonitoringResults.RemoveRange(olderThan);
            await _rtuContext.SaveChangesAsync();

            // get portion
            var portionEf = await _rtuContext.MonitoringResults
                .OrderBy(r => r.TimeStamp)
                .Take(portionSize)
                .ToListAsync();

            foreach (var monitoringResultEf in portionEf)
            {
                _logger.Info(Logs.RtuService, $"Made at {monitoringResultEf.TimeStamp} for trace {monitoringResultEf.TraceId} state is {monitoringResultEf.TraceState.ToString()}");
            }

            return portionEf.Select(r => r.FromEf()).ToList();
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, "GetPortionYoungerThan " + e.Message);
            return new List<MonitoringResultDto>();
        }
    }
}