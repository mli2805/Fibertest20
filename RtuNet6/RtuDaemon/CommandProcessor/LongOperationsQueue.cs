using System.Collections.Concurrent;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNet6;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuDaemon;

public class LongOperationsQueue
{
    private readonly ILogger<LongOperationsQueue> _logger;
    private ConcurrentDictionary<Guid, LongOperation> _longOperations = new ConcurrentDictionary<Guid, LongOperation>();

    public LongOperationsQueue(ILogger<LongOperationsQueue> logger)
    {
        _logger = logger;
    }

    public Guid EnqueueLongOperation(string json)
    {
        ToLog(json);
        var lo = new LongOperation(json);
        var result = _longOperations.TryAdd(lo.CommandGuid, lo);
        return result ? lo.CommandGuid : Guid.Empty;
    }

    public LongOperation? GetOldest()
    {
        return _longOperations.Values.ToArray().MinBy(o => o.QueuingTime);
    }

    public LongOperation? GetResult(Guid commandGuid)
    {
        var lo = _longOperations.TryGetValue(commandGuid, out var result);
        if (!lo)
        {
            _logger.Error(Logs.RtuService, "Command with such guid not found, maybe Service restarted since command has been received!");
            return null; // service restarted during long operation - think about this on server
        }

        if (result!.IsProcessed)
        {
            _longOperations.TryRemove(commandGuid, out _);
        }

        // result.IsProcessed could be FALSE - check this on server
        return result;
    }

    public void UpdateResult(Guid commandGuid, LongOperation newLo, LongOperation oldLo)
    {
        var result = _longOperations.TryUpdate(commandGuid, newLo, oldLo);
        _logger.Info(Logs.RtuService, $"UpdateResult: {result}");
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };
    private void ToLog(string json)
    {
        object? o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
        if (o == null) return;
        switch (o)
        {
            case InitializeRtuDto _: _logger.Info(Logs.RtuService, $"InitializeRtuDto received. {json}"); break;
            case ApplyMonitoringSettingsDto _: _logger.Info(Logs.RtuService, $"ApplyMonitoringSettingsDto received. {json}"); break;
        }
    }
}