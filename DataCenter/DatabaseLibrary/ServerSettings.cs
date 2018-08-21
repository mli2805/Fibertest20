using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class ServerSettings : ISettings
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private int _mysqlTcpPort;
        private string _measurementsScheme;

        public ServerSettings(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public void Init()
        {
            var doubleAddress = _iniFile.ReadDoubleAddress((int) TcpPorts.ServerListenToClient);
            if (doubleAddress.Main.IsAddressSetAsIp && doubleAddress.Main.Ip4Address == "0.0.0.0")
            {
                var serverIp = LocalAddressResearcher.GetAllLocalAddresses().First();
                _iniFile.Write(IniSection.ServerMainAddress, IniKey.Ip, serverIp);
            }

            _mysqlTcpPort = _iniFile.Read(IniSection.MySql, IniKey.MySqlTcpPort, 3306);
            var postfix = _iniFile.Read(IniSection.MySql, IniKey.MySqlDbSchemePostfix, "");
            _measurementsScheme = "ft20efcore" + postfix;
        }

        public void LogSettings()
        {
            _logFile.AppendLine($"Measurements: MYSQL=localhost:{_mysqlTcpPort}   Database={_measurementsScheme}");
        }

        private string MySqlConnectionString => $"server=localhost;port={_mysqlTcpPort};user id=root;password=root;database={_measurementsScheme}";

        public DbContextOptions<FtDbContext> Options =>
            new DbContextOptionsBuilder<FtDbContext>().UseMySql(MySqlConnectionString).Options;
    }
}