using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNet6;
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
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringPortRepository>();
        var allPorts = await repo.GetAll();

        var oldest = allPorts.MinBy(p => p.LastMadeTimestamp);
        return oldest;
    }

    private async Task<MonitoringPort?> GetMonitoringPort(string serial, int opticalPort)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringPortRepository>();
        var list = await repo.GetAll();
        return list.FirstOrDefault(p => p.CharonSerial == serial && p.OpticalPort == opticalPort);
    }


    private async Task UpdateMonitoringPort(MonitoringPort monitoringPort)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringPortRepository>();
        await repo.AddOrUpdate(monitoringPort);
    }

    private async Task<int> CreateNewQueue(List<PortWithTraceDto> ports)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringPortRepository>();
        var oldPorts = await repo.GetAll();

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

        await repo.ApplyNewList(newPorts);
        return newPorts.Count;
    }
}