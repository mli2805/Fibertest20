using Iit.Fibertest.Dto;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class TestParameterizer : IParameterizer
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;

        public TestParameterizer(CurrentDatacenterParameters currentDatacenterParameters)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            _currentDatacenterParameters.Smtp = new SmtpSettingsDto();
            _currentDatacenterParameters.Snmp = new SnmpSettingsDto();
        }

        public void Init()
        {
            _currentDatacenterParameters.Smtp = new SmtpSettingsDto();
        }
        public void LogSettings() { }

        public DbContextOptions<FtDbContext> Options => new DbContextOptionsBuilder<FtDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_database")
            .Options;
    }
}