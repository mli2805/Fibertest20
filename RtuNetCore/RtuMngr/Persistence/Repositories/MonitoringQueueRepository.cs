using AutoMapper;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public class MonitoringQueueRepository
{
    private static readonly IMapper Mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingEfProfile>()).CreateMapper();
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

        var newEntity = Mapper.Map<MonitoringPortEf>(monitoringPort);

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
        var ports = portEfs.Select(p => Mapper.Map<MonitoringPort>(p)).ToList();
        return ports;
    }

    public async Task<int> ApplyNewList(List<MonitoringPort> ports)
    {
        await using var transaction = await _rtuContext.Database.BeginTransactionAsync();

        try
        {
            var oldPorts = await _rtuContext.MonitoringQueue.ToListAsync();
            _logger.Info(Logs.RtuManager, $"ApplyNewList: db contains {oldPorts.Count} old ports");
            _rtuContext.MonitoringQueue.RemoveRange(oldPorts);

            var portEfs = ports.Select(Mapper.Map<MonitoringPortEf>).ToList();
            _logger.Info(Logs.RtuManager, $"ApplyNewList: {portEfs.Count} new ports were converted to EF form");
            await _rtuContext.AddRangeAsync(portEfs);
            var lines = await _rtuContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return lines;
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, "ApplyNewList: "+ e.Message);
            if (e.InnerException != null)
            {
                _logger.Error(Logs.RtuManager, "InnerException: "+ e.InnerException.Message);

            }
            await transaction.RollbackAsync();
            return -1;
        }
       
    }

}