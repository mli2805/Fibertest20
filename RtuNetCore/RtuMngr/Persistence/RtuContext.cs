using System.Reflection;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr
{
    public class RtuContext : DbContext
    {
#pragma warning disable CS8618
        public RtuContext(DbContextOptions<RtuContext> options) : base(options) { }
#pragma warning restore CS8618

        public DbSet<BopStateChangedEf> BopEvents { get; set; } // bop problems
        public DbSet<MonitoringResultEf> MonitoringResults { get; set; } // results and RTU_accidents
        public DbSet<ClientMeasurementEf> ClientMeasurements { get; set; }

        public DbSet<MonitoringPortEf> MonitoringQueue { get; set; }

        public DbSet<RtuSettingsEf> RtuSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder bulider)
        {
            bulider.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(bulider);
        }
    }

    public class RtuContextInitializer(ILogger<RtuContextInitializer> logger, RtuContext rtuContext)
    {
        public async Task InitializeAsync()
        {
            try
            {
                await rtuContext.Database.EnsureCreatedAsync();
                if (await rtuContext.RtuSettings.AnyAsync()) return;

                logger.Info(Logs.WatchDog, "Empty DB. ");
                logger.Info(Logs.RtuService, "Empty DB. ");
                {
                    rtuContext.RtuSettings.Add(
                        new RtuSettingsEf()
                        {
                            IsMonitoringOn = false,
                            IsAutoBaseMeasurementInProgress = false,

                            LastMeasurement = DateTime.MinValue,
                            LastAutoBase = DateTime.MinValue,
                            LastRestartByWatchDog = DateTime.MinValue,
                            LastCheckedByWatchDog = DateTime.MinValue,
                        });
                }
                await rtuContext.SaveChangesAsync();

            }
            catch (Exception e)
            {
                logger.Exception(Logs.WatchDog, e, "RtuContextInitializer");
                logger.Exception(Logs.RtuService, e, "RtuContextInitializer");
                throw;
            }
        }
    }
}
