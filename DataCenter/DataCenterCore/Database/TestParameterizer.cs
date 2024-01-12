using Iit.Fibertest.Dto;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DataCenterCore
{
    public class TestParameterizer : IParameterizer
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;

        public TestParameterizer(CurrentDatacenterParameters currentDatacenterParameters)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            _currentDatacenterParameters.Smtp = new SmtpSettingsDto();
            _currentDatacenterParameters.Snmp = new SnmpSettingsDto();
            _currentDatacenterParameters.Gsm = new GsmSettingsDto();
        }

        public void Init()
        {
            _currentDatacenterParameters.Smtp = new SmtpSettingsDto();
        }
        public void LogSettings() { }
        public string MySqlConnectionString => "";

        public DbContextOptions<FtDbContext> Options => new DbContextOptionsBuilder<FtDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_database")
            .Options;
    }
}