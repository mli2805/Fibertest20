namespace Iit.Fibertest.RtuMngr;

public class MonitoringResultsRepository
{
    private readonly RtuContext _rtuContext;

    public MonitoringResultsRepository(RtuContext rtuContext)
    {
        _rtuContext = rtuContext;
    }

    public async Task Add(MonitoringResultEf entity)
    {
        await _rtuContext.MonitoringResults.AddAsync(entity);
        await _rtuContext.SaveChangesAsync();
    }
}