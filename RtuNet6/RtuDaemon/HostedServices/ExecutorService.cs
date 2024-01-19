using System.Diagnostics;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNet6;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuDaemon;

public class ExecutorService : BackgroundService
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    private readonly ILogger<ExecutorService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly RtuManager _rtuManager;

    public ExecutorService(ILogger<ExecutorService> logger, IServiceProvider serviceProvider, RtuManager rtuManager)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _rtuManager = rtuManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;
        _logger.Info(Logs.RtuManager, $"RTU executor service started. Process {pid}, thread {tid}");

        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<LongOperationRepository>();
                var dto = await repo.ExtractForExecution();
                if (dto != null)
                {
                    _logger.Info(Logs.RtuService, "Long operation request found in Db");
                    var result = await Do(dto.Json);
                    //TODO: place result into DB
                }
                await Task.Delay(1000, token);
            }
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, "ExecutorService service DoWork: " + e.Message);
        }
    }

    private async Task<string?> Do(string json)
    {
        var o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);

        switch (o)
        {
            case InitializeRtuDto dto: 
                var result = await _rtuManager.InitializeRtu(dto, false);
                var resultJson = JsonConvert.SerializeObject(result, JsonSerializerSettings);
                _logger.Info(Logs.RtuService, $"Initialized successfully - {result.IsInitialized}");
                return resultJson;
        }

        return null;
    }
}