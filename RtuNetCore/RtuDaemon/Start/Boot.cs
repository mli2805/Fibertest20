using System.Diagnostics;
using System.Reflection;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Serilog.Events;

namespace Iit.Fibertest.RtuDaemon;

public sealed class Boot(IWritableConfig<RtuConfig> config, ILogger<Boot> logger) : IHostedService
{
    // Place here all that should be done before start listening to gRPC & Http requests, background workers, etc.
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);

        logger.StartLine(Logs.RtuService);
        logger.StartLine(Logs.RtuManager);
        logger.Info(Logs.RtuService, $"Fibertest RTU service {info.FileVersion}");
        logger.Info(Logs.RtuManager, $"Fibertest RTU service {info.FileVersion}");

        config.Update(c => c.General.LogEventLevel = LogEventLevel.Debug.ToString());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.Info(Logs.RtuService, "Leave Fibertest RTU service");
        return Task.CompletedTask;
    }
}