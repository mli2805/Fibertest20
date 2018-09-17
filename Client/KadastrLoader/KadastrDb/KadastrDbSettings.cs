using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace KadastrLoader
{
    public class KadastrDbSettings
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private string _mysqlServerAddress;
        private int _mysqlTcpPort;

        public KadastrDbSettings(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public void Init()
        {
            var doubleAddress = _iniFile.ReadDoubleAddress((int) TcpPorts.ServerListenToClient);
            _mysqlServerAddress = doubleAddress.Main.IsAddressSetAsIp
                ? doubleAddress.Main.Ip4Address
                : doubleAddress.Main.HostName;

            _mysqlTcpPort = _iniFile.Read(IniSection.MySql, IniKey.MySqlTcpPort, 33060);
            _logFile.AppendLine($"MySqlConnectionString = {MySqlConnectionString}");
        }
        private string MySqlConnectionString => $"server={_mysqlServerAddress};port={_mysqlTcpPort};user id=root;password=root;database=ft20kadastr";

        public DbContextOptions<KadastrDbContext> Options =>
            new DbContextOptionsBuilder<KadastrDbContext>().UseMySql(MySqlConnectionString).Options;
    }
}