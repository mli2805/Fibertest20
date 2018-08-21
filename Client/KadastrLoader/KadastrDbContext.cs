using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace KadastrLoader
{
    public class KadastrDbContext : DbContext
    {
        public KadastrDbContext(DbContextOptions<KadastrDbContext> options) : base(options) { }

        public DbSet<Well> Wells { get; set; }
    }

    public class Well
    {
        public int Id { get; set; }
        public int InKadastrId { get; set; }
        public Guid InFibertestId { get; set; }
    }

    public class KadastrDbSettings
    {
        private readonly IniFile _iniFile;
        private string _mysqlServerAddress;
        private int _mysqlTcpPort;

        public KadastrDbSettings(IniFile iniFile)
        {
            _iniFile = iniFile;
        }

        public void Init()
        {
            var doubleAddress = _iniFile.ReadDoubleAddress((int) TcpPorts.ServerListenToClient);
            _mysqlServerAddress = doubleAddress.Main.IsAddressSetAsIp
                ? doubleAddress.Main.Ip4Address
                : doubleAddress.Main.HostName;

            _mysqlTcpPort = _iniFile.Read(IniSection.MySql, IniKey.MySqlTcpPort, 3306);
        }
        private string MySqlConnectionString => $"server={_mysqlServerAddress};port={_mysqlTcpPort};user id=root;password=root;database=ft20kadastr";

        public DbContextOptions<KadastrDbContext> Options =>
            new DbContextOptionsBuilder<KadastrDbContext>().UseMySql(MySqlConnectionString).Options;
    }
}
