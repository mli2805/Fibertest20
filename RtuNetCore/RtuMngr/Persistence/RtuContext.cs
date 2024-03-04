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
            }
            catch (Exception e)
            {
                logger.Exception(Logs.RtuService, e, "RtuContextInitializer");
                throw;
            }
        }
    }
}
