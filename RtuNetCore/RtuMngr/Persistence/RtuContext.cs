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

        public DbSet<DtoInDbEf> Events { get; set; } // results and
        public DbSet<MonitoringResultEf> MonitoringResults { get; set; }
        public DbSet<ClientMeasurementEf> ClientMeasurements { get; set; }

        // 4 tables => MonitoringQueue + MoniResultEf, MoniLevelEf, AccidentInSorEf
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
                logger.Error(Logs.RtuService, "An error occurred while initializing the database.");
                logger.Error(Logs.RtuService, e.Message);
                throw;
            }
        }
    }
}
