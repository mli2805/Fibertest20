using System.Diagnostics;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class ServerParameterizer : IParameterizer
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private int _mysqlTcpPort;
        private string _measurementsScheme;

        public ServerParameterizer(IniFile iniFile, IMyLog logFile, CurrentDatacenterParameters currentDatacenterParameters)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _currentDatacenterParameters = currentDatacenterParameters;
        }

        public void Init()
        {
            var doubleAddress = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
            if (doubleAddress.Main.IsAddressSetAsIp && doubleAddress.Main.Ip4Address == "0.0.0.0")
            {
                var serverIp = LocalAddressResearcher.GetAllLocalAddresses().First();
                _iniFile.Write(IniSection.ServerMainAddress, IniKey.Ip, serverIp);
            }

            _currentDatacenterParameters.ServerIp = doubleAddress.Main.Ip4Address;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            _currentDatacenterParameters.DatacenterVersion = fvi.FileVersion;

            _currentDatacenterParameters.WebApiDomainName = _iniFile.Read(IniSection.WebApi, IniKey.DomainName, "iit-fibertest");
            _currentDatacenterParameters.WebApiBindingProtocol = _iniFile.Read(IniSection.WebApi, IniKey.BindingProtocol, "none");
            if (_currentDatacenterParameters.WebApiBindingProtocol == "none")
                _logFile.AppendLine("Web API service is not installed.");

            _currentDatacenterParameters.Smtp = new SmtpSettingsDto()
            {
                SmptHost = _iniFile.Read(IniSection.Smtp, IniKey.SmtpHost, ""),
                SmptPort = _iniFile.Read(IniSection.Smtp, IniKey.SmtpPort, 0),
                MailFrom = _iniFile.Read(IniSection.Smtp, IniKey.MailFrom, ""),
                MailFromPassword = _iniFile.Read(IniSection.Smtp, IniKey.MailFromPassword, ""),
                SmtpTimeoutMs = _iniFile.Read(IniSection.Smtp, IniKey.SmtpTimeoutMs, 0),
            };
            _currentDatacenterParameters.GsmModemComPort = _iniFile.Read(IniSection.Broadcast, IniKey.GsmModemComPort, 0);

            var localIp = LocalAddressResearcher.GetAllLocalAddresses().FirstOrDefault() ?? "127.0.0.1";
            _currentDatacenterParameters.Snmp = new SnmpSettingsDto()
            {
                IsSnmpOn = _iniFile.Read(IniSection.Snmp, IniKey.IsSnmpOn, true),
                SnmpReceiverIp = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverIp, "192.168.96.21"),
                SnmpReceiverPort = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverPort, 162),
                SnmpAgentIp = _iniFile.Read(IniSection.Snmp, IniKey.SnmpAgentIp, localIp),
                SnmpCommunity = _iniFile.Read(IniSection.Snmp, IniKey.SnmpCommunity, "IIT"),
                SnmpEncoding = _iniFile.Read(IniSection.Snmp, IniKey.SnmpEncoding, "windows1251"),
                EnterpriseOid = _iniFile.Read(IniSection.Snmp, IniKey.EnterpriseOid, "1.3.6.1.4.1.36220"),
            };

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