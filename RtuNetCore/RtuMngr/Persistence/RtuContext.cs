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
        public DbSet<MonitoringPortEf> MonitoringPorts { get; set; }
        public DbSet<LongOperationEf> LongOperations { get; set; } // request to execute

        protected override void OnModelCreating(ModelBuilder bulider)
        {
            bulider.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(bulider);
        }
    }

    public class RtuContextInitializer
    {
        private readonly ILogger<RtuContextInitializer> _logger;
        private readonly RtuContext _rtuContext;

        public RtuContextInitializer(ILogger<RtuContextInitializer> logger, RtuContext rtuContext)
        {
            _logger = logger;
            _rtuContext = rtuContext;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _rtuContext.Database.EnsureCreatedAsync();
            }
            catch (Exception e)
            {
                _logger.Error(Logs.RtuService, "An error occurred while initializing the database.");
                _logger.Error(Logs.RtuService, e.Message);
                throw;
            }
        }
    }
}
