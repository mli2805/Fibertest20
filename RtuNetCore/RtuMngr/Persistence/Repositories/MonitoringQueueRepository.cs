using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public class MonitoringQueueRepository
{
    private readonly RtuContext _rtuContext;
    private readonly ILogger<MonitoringQueueRepository> _logger;

    public MonitoringQueueRepository(RtuContext rtuContext, ILogger<MonitoringQueueRepository> logger)
    {
        _rtuContext = rtuContext;
        _logger = logger;
    }

    public async Task AddOrUpdate(MonitoringPort monitoringPort)
    {
        var entity = await _rtuContext.MonitoringQueue.FirstOrDefaultAsync(p =>
            p.CharonSerial == monitoringPort.CharonSerial && p.OpticalPort == monitoringPort.OpticalPort);

        var newEntity = monitoringPort.ToEf();

        if (entity != null)
        {
            await UpdateExistingMonitoringPort(entity, newEntity);
        }
        else
        {
            await _rtuContext.MonitoringQueue.AddAsync(newEntity);
            await _rtuContext.SaveChangesAsync();
        }
    }

    private async Task UpdateExistingMonitoringPort(MonitoringPortEf entity, MonitoringPortEf newEntity)
    {
        await using var transaction = await _rtuContext.Database.BeginTransactionAsync();

        try
        {
            _rtuContext.MonitoringQueue.Remove(entity);
            _rtuContext.MonitoringQueue.Add(newEntity);
            await _rtuContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
        }
    }

    public async Task<List<MonitoringPort>> GetAll()
    {
        var portEfs = await _rtuContext.MonitoringQueue.ToListAsync();
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
        await using var transaction = await _rtuContext.Database.BeginTransactionAsync();

        try
        {
            var oldPorts = await _rtuContext.MonitoringQueue.ToListAsync();
            _rtuContext.MonitoringQueue.RemoveRange(oldPorts);

            var portEfs = ports.Select(p=>p.ToEf()).ToList();
            await _rtuContext.AddRangeAsync(portEfs);
            await _rtuContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, "ApplyNewList: " + e.Message);
            if (e.InnerException != null)
                _logger.Error(Logs.RtuManager, "InnerException: " + e.InnerException.Message);
            await transaction.RollbackAsync();
        }

    }

}