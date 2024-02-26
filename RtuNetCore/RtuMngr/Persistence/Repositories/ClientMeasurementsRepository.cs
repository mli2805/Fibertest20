using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public class ClientMeasurementsRepository(RtuContext rtuContext, ILogger<ClientMeasurementsRepository> logger)
{
    public async Task Add(ClientMeasurementEf entity)
    {
        logger.Info(Logs.RtuManager, $"persist client measurement result {entity.ReturnCode}");
        try
        {
            rtuContext.ClientMeasurements.Add(entity);
            await rtuContext.SaveChangesAsync();
            logger.Info(Logs.RtuManager, $"Successfully persisted client measurement result {entity.ReturnCode}");
        }
        catch (Exception e)
        {
            logger.Exception(Logs.RtuManager, e, "Persist measurement (Client)");
        }
    }

    public async Task<List<ClientMeasurementEf>> GetAll()
    {
        var all = await rtuContext.ClientMeasurements.ToListAsync();
        rtuContext.ClientMeasurements.RemoveRange(all);
        await rtuContext.SaveChangesAsync();
        return all;
    }
}