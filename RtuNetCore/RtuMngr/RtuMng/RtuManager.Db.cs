using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuMngr;

public partial class RtuManager
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };
    // ClientMeasurementResultDto, BopStateChangedDto, ??? rtu accidents how?
    private async Task SaveMoniResult(MonitoringResultEf entity)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringResultsRepository>();
        await repo.Add(entity);
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
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringQueueRepository>();
        var allPorts = await repo.GetAll();
        _logger.Info(Logs.RtuManager, $"Queue contains {allPorts.Count} entries");
        return allPorts.Count == 0 ? null : allPorts.MinBy(p => p.LastMadeTimestamp);
    }

    private async Task<MonitoringPort?> GetMonitoringPort(string serial, int opticalPort)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringQueueRepository>();
        var list = await repo.GetAll();
        return list.FirstOrDefault(p => p.CharonSerial == serial && p.OpticalPort == opticalPort);
    }


    private async Task UpdateMonitoringPort(MonitoringPort monitoringPort)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringQueueRepository>();
        await repo.AddOrUpdate(monitoringPort);
    }

    private async Task<int> CreateNewQueue(List<PortWithTraceDto> ports)
    {
        _logger.Info(Logs.RtuManager, $"User sent {ports.Count} ports for monitoring");

        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringQueueRepository>();
        var oldPorts = await repo.GetAll();
        _logger.Info(Logs.RtuManager, $"Queue contains {ports.Count} ports");

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

        _logger.Info(Logs.RtuManager, $"Save new queue with {ports.Count} ports");
        var portCount =  await repo.ApplyNewList(newPorts);
        _logger.Info(Logs.RtuManager, $"Db reported: {portCount} ports saved in queue");
        return newPorts.Count;
    }
}