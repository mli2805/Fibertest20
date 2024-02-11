using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public class ClientMeasurementsRepository(RtuContext rtuContext, ILogger<ClientMeasurementsRepository> logger)
{
    public async Task Add(ClientMeasurementEf entity)
    {
        logger.Info(Logs.RtuManager, $"persist client measurement result {entity.ReturnCode}");
        await rtuContext.ClientMeasurements.AddAsync(entity);
        await rtuContext.SaveChangesAsync();
    }

    public async Task<List<ClientMeasurementEf>> GetAll()
    {
        var all = await rtuContext.ClientMeasurements.ToListAsync();
        rtuContext.ClientMeasurements.RemoveRange(all);
        await rtuContext.SaveChangesAsync();
        return all;
    }
}