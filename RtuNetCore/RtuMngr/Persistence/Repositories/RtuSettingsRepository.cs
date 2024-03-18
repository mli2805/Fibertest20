using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public class RtuSettingsRepository(RtuContext rtuContext, ILogger<BopEventsRepository> logger)
{
    // if NULL just returns current value
    public async Task<bool?> GetOrUpdateIsMonitoringOn(bool? newValue)
    {
        try
        {
            var line = await rtuContext.RtuSettings.FirstOrDefaultAsync();
            if (line == null)
            {
                line = new RtuSettingsEf()
                {
                    LastMeasurement = DateTime.MinValue,
                    LastAutoBase = DateTime.MinValue,
                    LastCheckedByWatchDog = DateTime.Now,
                };
                rtuContext.RtuSettings.Add(line);
            }

            if (newValue != null)
            {
                line.IsMonitoringOn = (bool)newValue;
                await rtuContext.SaveChangesAsync();
            }
            return line.IsMonitoringOn;
        }
        catch (Exception e)
        {
            logger.Exception(Logs.WatchDog, e, "GetOrUpdateIsMonitoringOn");
            return false;
        }
    }

    public async Task<bool?> GetOrUpdateIsAutoBaseMeasurementInProgress(bool? newValue)
    {
        try
        {
            var line = await rtuContext.RtuSettings.SingleAsync();
            if (newValue != null)
            {
                line.IsAutoBaseMeasurementInProgress = (bool)newValue;
                await rtuContext.SaveChangesAsync();
            }
            return line.IsAutoBaseMeasurementInProgress;
        }
        catch (Exception e)
        {
            logger.Exception(Logs.WatchDog, e, "GetOrUpdateIsAutoBaseMeasurementInProgress");
            return false;
        }
    }

    public async Task UpdateLastMeasurement()
    {
        try
        {
            var line = await rtuContext.RtuSettings.SingleAsync();
            line.LastMeasurement = DateTime.Now;
            await rtuContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.Exception(Logs.WatchDog, e, "UpdateLastMeasurement");
        }
    }

    public async Task UpdateLastAutoBase()
    {
        try
        {
            var line = await rtuContext.RtuSettings.SingleAsync();
            line.LastAutoBase = DateTime.Now;
            await rtuContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.Exception(Logs.WatchDog, e, "UpdateLastAutoBase");
        }
    }

    public async Task SetRestartedTimestamp()
    {
        try
        {
            var line = await rtuContext.RtuSettings.SingleAsync();
            line.LastRestartByWatchDog = DateTime.Now;
            await rtuContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.Exception(Logs.WatchDog, e, "SetRestartedTimestamp");
        }
    }

    public async Task<DateTime> CheckByWatchDog(bool lastMeasurement)
    {
        try
        {
            var line = await rtuContext.RtuSettings.FirstOrDefaultAsync();
            if (line == null)
            {
                line = new RtuSettingsEf()
                {
                    LastMeasurement = DateTime.MinValue,
                    LastAutoBase = DateTime.MinValue,
                    LastCheckedByWatchDog = DateTime.Now,
                };
                rtuContext.RtuSettings.Add(line);
            }

            line.LastCheckedByWatchDog = DateTime.Now;
            await rtuContext.SaveChangesAsync();

            return lastMeasurement ? line.LastMeasurement : line.LastAutoBase;
        }
        catch (Exception e)
        {
            logger.Exception(Logs.WatchDog, e, "CheckByWatchDog");
            return DateTime.MinValue;
        }
    }


}