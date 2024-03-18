using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Iit.Fibertest.RtuMngr;

public partial class RtuManager
{
    public async Task<bool> GetIsMonitoringOn()
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<RtuSettingsRepository>();
        return (bool)(await repo.GetOrUpdateIsMonitoringOn(null))!;
    }

    private async Task UpdateIsMonitoringOn(bool newValue)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<RtuSettingsRepository>();
        _ = await repo.GetOrUpdateIsMonitoringOn(newValue);
    }

    public async Task<bool> GetIsAutoBaseMeasurementInProgress()
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<RtuSettingsRepository>();
        return (bool)(await repo.GetOrUpdateIsAutoBaseMeasurementInProgress(null))!;
    }

    private async Task UpdateIsAutoBaseMeasurementInProgress(bool newValue)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<RtuSettingsRepository>();
        _ = await repo.GetOrUpdateIsAutoBaseMeasurementInProgress(newValue);
    }

    private async Task PersistMoniResultForServer(MonitoringResultEf entity)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<MonitoringResultsRepository>();
            await repo.Add(entity);
        }
        catch (Exception e)
        {
            _logger.Exception(Logs.RtuManager, e, "PersistMoniResultForServer");
        }
    }

    private async Task PersistClientMeasurementResult(ClientMeasurementResultDto dto)
    {
        try
        {
            var entity = dto.ToEf();
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ClientMeasurementsRepository>();
            await repo.Add(entity);
        }
        catch (Exception e)
        {
            _logger.Exception(Logs.RtuManager, e, "PersistClientMeasurementResult");
        }
    }

    private async Task SaveBopEvent(BopStateChangedDto dto)
    {
        try
        {
            var entity = dto.ToEf();
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<BopEventsRepository>();
            await repo.Add(entity);
        }
        catch (Exception e)
        {
            _logger.Exception(Logs.RtuManager, e, "SaveBopEvent");
        }
    }

    private async Task<MonitoringPort?> GetNextPortForMonitoring()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<MonitoringQueueRepository>();
            var allPorts = await repo.GetAll();
            return allPorts.Count == 0 ? null : allPorts.MinBy(p => p.LastMadeTimestamp);
        }
        catch (Exception e)
        {
            _logger.Exception(Logs.RtuManager, e, "GetNextPortForMonitoring");
            return null;
        }
    }

    private async Task<MonitoringPort?> GetMonitoringPort(string serial, int opticalPort)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringQueueRepository>();
        var list = await repo.GetAll();
        return list.FirstOrDefault(p => p.CharonSerial == serial && p.OpticalPort == opticalPort);
    }


    private async Task PersistMonitoringPort(MonitoringPort monitoringPort)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringQueueRepository>();
        await repo.AddOrUpdate(monitoringPort);
    }

    private async Task PersistLastMeasurementTime()
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<RtuSettingsRepository>();
        await repo.UpdateLastMeasurement();
    }

    private async Task PersistLastAutoBaseTime()
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<RtuSettingsRepository>();
        await repo.UpdateLastAutoBase();
    }

    private async Task<int> CreateNewQueue(List<PortWithTraceDto> ports)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringQueueRepository>();
        return await repo.CreateNewQueue(ports);
    }
}