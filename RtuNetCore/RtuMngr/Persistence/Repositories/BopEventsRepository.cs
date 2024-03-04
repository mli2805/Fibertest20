using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public class BopEventsRepository(RtuContext rtuContext, ILogger<BopEventsRepository> logger)
{
    public async Task Add(BopStateChangedEf entity)
    {
        logger.Info(Logs.RtuManager, $"persist bop {entity.Serial} on {entity.OtauIp}:{entity.TcpPort} problem");
        try
        {
            rtuContext.BopEvents.Add(entity);
            await rtuContext.SaveChangesAsync();
            logger.Info(Logs.RtuManager, "Successfully persisted bop problem");
        }
        catch (Exception e)
        {
            logger.Exception(Logs.RtuManager, e, "Persist bop problem");
        }
    }

    public async Task<List<BopStateChangedEf>> GetAll()
    {
        var all = await rtuContext.BopEvents.ToListAsync();
        rtuContext.BopEvents.RemoveRange(all);
        await rtuContext.SaveChangesAsync();
        return all;
    }
}