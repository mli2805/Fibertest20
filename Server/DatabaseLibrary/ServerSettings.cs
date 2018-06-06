using System.Linq;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
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
            var address = _iniFile.Read(IniSection.ServerMainAddress, IniKey.Ip, "0.0.0.0");
            if (address == "0.0.0.0")
            {
                var serverIp = LocalAddressResearcher.GetAllLocalAddresses().First();
                _iniFile.Write(IniSection.ServerMainAddress, IniKey.Ip, serverIp);
            }
            _mysqlTcpPort = _iniFile.Read(IniSection.MySql, IniKey.MySqlTcpPort, 3306);
            var postfix = _iniFile.Read(IniSection.MySql, IniKey.MySqlDbSchemePostfix, "");
            _measurementsScheme = "ft20efcore" + postfix;
        }

        private string MySqlConnectionString => $"server=localhost;port={_mysqlTcpPort};user id=root;password=root;database={_measurementsScheme}";

        public DbContextOptions<FtDbContext> Options =>
            new DbContextOptionsBuilder<FtDbContext>().UseMySql(MySqlConnectionString).Options;
    }
}