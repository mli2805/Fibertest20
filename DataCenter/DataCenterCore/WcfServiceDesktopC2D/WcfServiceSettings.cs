using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Utils471;

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

        public Task<bool> TestSnmpNewSettings(SnmpNewSettingsDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var newSnmpSettings = JsonSerializer.Deserialize<SnmpNewSettings>(json);

            // var message = "Тестовая строка полностью на русском языке.";
            var message = "Test string with Русский язык.";
            var payload = new Dictionary<FtTrapProperty, string>()
            {
                { FtTrapProperty.TestString, message },
                { FtTrapProperty.EventRegistrationTime, DateTime.Now.ToString(CultureInfo.InvariantCulture) },
                { FtTrapProperty.TestInt, 123.ToString() },
                { FtTrapProperty.TestDouble, 3.1415926.ToString(CultureInfo.InvariantCulture) }
            };

            _snmpService.SendSnmpTrap(newSnmpSettings, FtTrapType.TestTrap, payload);
            return Task.FromResult(true);
        }

        public Task<bool> SaveGisMode(bool isWithoutMapMode)
        {
            _logFile.AppendLine("Client asked to save GIS mode");
            _iniFile.Write(IniSection.Server, IniKey.IsWithoutMapMode, isWithoutMapMode);
            return Task.FromResult(true);
        }

        public Task<bool> SaveGsmSettings(GsmSettingsDto dto)
        {
            _logFile.AppendLine("Client asked to save GSM settings");
            _smsManager.SaveGsmSettings(dto);
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
