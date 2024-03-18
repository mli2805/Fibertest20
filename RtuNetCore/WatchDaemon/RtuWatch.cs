using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNetCore;

namespace Iit.Fibertest.WatchDaemon;

public class RtuWatch(IWritableConfig<WatchDogConfig> config, ILogger<RtuWatch> logger,
    DebianServiceManager debianServiceManager, IServiceProvider serviceProvider)
{

    public async Task StartWatchCycle()
    {
        while (true)
        {
            //logger.Debug(Logs.WatchDog, "Tick");
            await Tick();
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private async Task Tick()
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<RtuSettingsRepository>();

        if (debianServiceManager.CheckServiceRunning(config.Value.RtuDaemonExecutiveName) == 0)
        {
            logger.Info(Logs.WatchDog, "Start it!");
            debianServiceManager.ServiceCommand(config.Value.RtuDaemonName, "start");
            await repository.SetRestartedTimestamp();
            Thread.Sleep(TimeSpan.FromSeconds(90));

            return;
        }

        if ((bool)(await repository.GetOrUpdateIsMonitoringOn(null))!)
        {
            //logger.Debug(Logs.WatchDog, "Monitoring is ON, check last measurement timestamp");
            await CheckLastMeasurement(repository);
        }

        if ((bool)(await repository.GetOrUpdateIsAutoBaseMeasurementInProgress(null))!)
        {
            logger.Debug(Logs.WatchDog, "Auto base measurements in progress, check last auto base measurement timestamp");
            await CheckLastAutoBase(repository);
        }
    }

    private async Task CheckLastMeasurement(RtuSettingsRepository repository)
    {
        var timestamp = await repository.CheckByWatchDog(true);
        //logger.Info(Logs.WatchDog, $"Last measurement {timestamp}");

        if ((DateTime.Now - timestamp).TotalSeconds > config.Value.MaxGapBetweenMeasurements)
        {
            logger.Error(Logs.WatchDog, $"IsMonitoringOn = true but last measurement time is {timestamp}");
            logger.Info(Logs.WatchDog, "Restart RTU daemon!");
            debianServiceManager.ServiceCommand(config.Value.RtuDaemonName, "stop");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }

    private async Task CheckLastAutoBase(RtuSettingsRepository repository)
    {
        var timestamp = await repository.CheckByWatchDog(false);
        logger.Info(Logs.WatchDog, $"Last auto base measurement {timestamp}");

        if ((DateTime.Now - timestamp).TotalSeconds > config.Value.MaxGapBetweenAutoBaseMeasurements)
        {
            logger.Error(Logs.WatchDog, $"AutoBaseMeasurementInProgress = true but last auto base measurement time is {timestamp}");
            logger.Info(Logs.WatchDog, "Restart RTU daemon!");
            debianServiceManager.ServiceCommand(config.Value.RtuDaemonName, "stop");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }

}