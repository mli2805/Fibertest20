using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class FtDbContext : DbContext
    {
        public FtDbContext()  { }
        public FtDbContext(DbContextOptions<FtDbContext> options) : base(options) { }


        public DbSet<ClientStation> ClientStations { get; set; }
        public DbSet<RtuStation> RtuStations { get; set; }
        public DbSet<SorFile> SorFiles { get; set; }
    }

    public interface ISettings
    {
        void Init();
        DbContextOptions<FtDbContext> Options { get; }

    }
    public class ServerSettings : ISettings
    {
        private readonly IniFile _iniFile;
        private int _mysqlTcpPort;
        private string _measurementsScheme;

        public ServerSettings(IniFile iniFile)
        {
            _iniFile = iniFile;
        }

        public void Init()
        {
            _mysqlTcpPort = _iniFile.Read(IniSection.MySql, IniKey.MySqlTcpPort, 3306);
            var postfix = _iniFile.Read(IniSection.MySql, IniKey.MySqlDbSchemePostfix, "");
            _measurementsScheme = "ft20efcore" + postfix;
        }

        private string MySqlConnectionString => $"server=localhost;port={_mysqlTcpPort};user id=root;password=root;database={_measurementsScheme}";

        public DbContextOptions<FtDbContext> Options =>
            new DbContextOptionsBuilder<FtDbContext>().UseMySql(MySqlConnectionString).Options;
    }
    public class TestSettings : ISettings
    {
        public void Init() { }
        public DbContextOptions<FtDbContext> Options => new DbContextOptionsBuilder<FtDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_database")
            .Options;
    }
}