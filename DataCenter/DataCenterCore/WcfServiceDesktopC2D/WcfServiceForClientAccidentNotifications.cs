using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceDesktopC2D
    {
        public Task<bool> SaveSmtpSettings(SmtpSettingsDto dto)
        {
            _logFile.AppendLine("Client asked to save SMTP settings");
            _smtp.SaveSmtpSettings(dto);
            return Task.FromResult(true);
        }
        public Task<bool> SaveSnmpSettings(SnmpSettingsDto dto)
        {
            _logFile.AppendLine("Client asked to save SNMP settings");
            _snmpAgent.SaveSnmpSettings(dto);

            _snmpAgent.SendTestTrap();

            return Task.FromResult(true);
        }

        public Task<bool> SaveGisMode(bool isWithoutMapMode)
        {
            _logFile.AppendLine("Client asked to save GIS mode");
            _iniFile.Write(IniSection.Server, IniKey.IsWithoutMapMode, isWithoutMapMode);
            return Task.FromResult(true);
        }

        public Task<bool> SaveGsmComPort(int comPort)
        {
            _logFile.AppendLine("Client asked to save com port");
            _iniFile.Write(IniSection.Broadcast, IniKey.GsmModemComPort, comPort);
            _currentDatacenterParameters.GsmModemComPort = comPort;
            return Task.FromResult(true);
        }

        public Task<bool> SendTest(string to, NotificationType notificationType)
        {
            var cu = _iniFile.Read(IniSection.General, IniKey.Culture, "ru-RU");
            var currentCulture = new CultureInfo(cu);
            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return notificationType == NotificationType.Email ? _smtp.SendTest(to) : _smsManager.SendTest(to);
        }
    }
}
