using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuMngr;

public partial class RtuManager
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };
    //  BopStateChangedDto ?? 

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

    private async Task SaveEvent(object obj)
    {
        var json = JsonConvert.SerializeObject(obj, JsonSerializerSettings);
        _logger.Info(Logs.RtuService, json);

        using var scope = _serviceProvider.CreateScope();
        var eventsRepository = scope.ServiceProvider.GetRequiredService<EventsRepository>();
        await eventsRepository.Add(json);
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

    private async Task<int> CreateNewQueue(List<PortWithTraceDto> ports)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringQueueRepository>();
        return await repo.CreateNewQueue(ports);
    }
}