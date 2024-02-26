using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public class MonitoringQueueRepository(RtuContext rtuContext, ILogger<MonitoringQueueRepository> logger)
{
    public async Task AddOrUpdate(MonitoringPort monitoringPort)
    {
        var entity = await rtuContext.MonitoringQueue.FirstOrDefaultAsync(p =>
            p.CharonSerial == monitoringPort.CharonSerial && p.OpticalPort == monitoringPort.OpticalPort);

        MonitoringPortEf newEntity = monitoringPort.ToEf();

        if (entity != null)
        {
            await UpdateExistingMonitoringPort(entity, newEntity);
        }
        else
        {
            rtuContext.MonitoringQueue.Add(newEntity);
            await rtuContext.SaveChangesAsync();
        }
    }

    private async Task UpdateExistingMonitoringPort(MonitoringPortEf entity, MonitoringPortEf newEntity)
    {
        await using var transaction = await rtuContext.Database.BeginTransactionAsync();

        try
        {
            rtuContext.MonitoringQueue.Remove(entity);
            rtuContext.MonitoringQueue.Add(newEntity);
            await rtuContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            logger.Exception(Logs.RtuManager, e, "UpdateExistingMonitoringPort");
            await transaction.RollbackAsync();
        }
    }

    public async Task<List<MonitoringPort>> GetAll()
    {
        var portEfs = await rtuContext.MonitoringQueue.ToListAsync();
        var ports = portEfs.Select(p => p.FromEf()).ToList();
        return ports;
    }

    public async Task<int> CreateNewQueue(List<PortWithTraceDto> ports)
    {
        var oldPorts = await GetAll();
        var newPorts = new List<MonitoringPort>();
        foreach (var portWithTraceDto in ports)
        {
            var monitoringPort = new MonitoringPort(portWithTraceDto);
            var oldPort = oldPorts.FirstOrDefault(p => p.TraceId == monitoringPort.TraceId);
            if (oldPort != null)
            {
                monitoringPort.LastFastSavedTimestamp = oldPort.LastFastSavedTimestamp;
                monitoringPort.LastPreciseSavedTimestamp = oldPort.LastPreciseSavedTimestamp;
                monitoringPort.LastFastMadeTimestamp = oldPort.LastFastMadeTimestamp;
                monitoringPort.LastPreciseMadeTimestamp = oldPort.LastPreciseMadeTimestamp;
            }
            newPorts.Add(monitoringPort);
        }

        await ApplyNewList(newPorts);
        return newPorts.Count;
    }

    private async Task ApplyNewList(List<MonitoringPort> ports)
    {
        await using var transaction = await rtuContext.Database.BeginTransactionAsync();

        try
        {
            var oldPorts = await rtuContext.MonitoringQueue.ToListAsync();
            rtuContext.MonitoringQueue.RemoveRange(oldPorts);

            var portEfs = ports.Select(p => p.ToEf()).ToList();
            await rtuContext.AddRangeAsync(portEfs);
            await rtuContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            logger.Exception(Logs.RtuManager, e, "ApplyNewList");
            await transaction.RollbackAsync();
        }

    }

}