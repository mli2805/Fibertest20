using System.Diagnostics;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNetCore;

namespace Iit.Fibertest.RtuDaemon;

public class MonitoringService(ILogger<MonitoringService> logger, RtuManager rtuManager)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;
        logger.Info(Logs.RtuManager, $"RTU monitoring service started. Process {pid}, thread {tid}");

        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        rtuManager.RtuServiceCancellationToken = stoppingToken;
        var result = await rtuManager.InitializeRtu(null, !(await rtuManager.GetIsMonitoringOn()));
        if (result.ReturnCode != ReturnCode.RtuInitializedSuccessfully)
            return;
        if (await rtuManager.GetIsMonitoringOn())
            await rtuManager.RunMonitoringCycle();
    }
}