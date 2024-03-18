using System.Diagnostics;
using System.Reflection;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Serilog.Events;

namespace Iit.Fibertest.WatchDaemon
{
    public sealed class Boot(IWritableConfig<WatchDogConfig> config, ILogger<Boot> logger, RtuWatch rtuWatch) : IHostedService
    {
        // Place here all that should be done before start listening to gRPC & Http requests, background workers, etc.
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);

            logger.StartLine(Logs.WatchDog);
            logger.Info(Logs.WatchDog, $"Fibertest WatchDog service {info.FileVersion}");

            config.Update(c => c.LogEventLevel = LogEventLevel.Debug.ToString());

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var watchThread = new Thread(() => rtuWatch.StartWatchCycle()) { IsBackground = true };
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            watchThread.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Info(Logs.WatchDog, "Leave Fibertest WatchDog service");
            return Task.CompletedTask;
        }
    }
}
