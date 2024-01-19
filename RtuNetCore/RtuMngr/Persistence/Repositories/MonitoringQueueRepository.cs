using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.RtuMngr;

public class MonitoringQueueRepository
{
    private static readonly IMapper Mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingEfProfile>()).CreateMapper();
    private readonly RtuContext _rtuContext;

    public MonitoringQueueRepository(RtuContext rtuContext)
    {
        _rtuContext = rtuContext;
    }

    public async Task AddOrUpdate(MonitoringPort monitoringPort)
    {
        var entity = await _rtuContext.MonitoringPorts.FirstOrDefaultAsync(p =>
            p.CharonSerial == monitoringPort.CharonSerial && p.OpticalPort == monitoringPort.OpticalPort);

        var newEntity = Mapper.Map<MonitoringPortEf>(monitoringPort);

        if (entity != null)
        {
            await UpdateExistingMonitoringPort(entity, newEntity);
        }
        else
        {
            await _rtuContext.MonitoringPorts.AddAsync(newEntity);
            await _rtuContext.SaveChangesAsync();
        }
    }

    private async Task UpdateExistingMonitoringPort(MonitoringPortEf entity, MonitoringPortEf newEntity)
    {
        await using var transaction = await _rtuContext.Database.BeginTransactionAsync();

        try
        {
            _rtuContext.MonitoringPorts.Remove(entity);
            _rtuContext.MonitoringPorts.Add(newEntity);
            await _rtuContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
        }
    }

    public async Task<List<MonitoringPort>> GetAll()
    {
        var portEfs = await _rtuContext.MonitoringPorts.ToListAsync();
        var ports = portEfs.Select(p => Mapper.Map<MonitoringPort>(p)).ToList();
        return ports;
    }

    public async Task ApplyNewList(List<MonitoringPort> ports)
    {
        await using var transaction = await _rtuContext.Database.BeginTransactionAsync();

        try
        {
            var oldPorts = await _rtuContext.MonitoringPorts.ToListAsync();
            _rtuContext.MonitoringPorts.RemoveRange(oldPorts);

            var portEfs = ports.Select(p => Mapper.Map<MonitoringPortEf>(p));
            await _rtuContext.AddRangeAsync(portEfs);
            await _rtuContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
        }
       
    }

}