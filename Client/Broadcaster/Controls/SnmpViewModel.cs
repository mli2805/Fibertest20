using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Caliburn.Micro;
using Utils471;
using System.Text.Json;
using Iit.Fibertest.Dto;

namespace Broadcaster
{
    public class SnmpViewModel : PropertyChangedBase
    {
        private readonly SnmpService _snmpService;

        public List<string> SnmpVersions { get; set; } = new List<string>() { @"v1", @"v3" };

        private string _selectedSnmpVersion;
        public string SelectedSnmpVersion
        {
            get => _selectedSnmpVersion;
            set
            {
                if (value == _selectedSnmpVersion) return;
                _selectedSnmpVersion = value;
                NotifyOfPropertyChange();
            }
        }


        private bool _useIitOid;
        public bool UseIitOid
        {
            get => _useIitOid;
            set
            {
                if (value == _useIitOid) return;
                _useIitOid = value;
                EnableCustomId = !_useIitOid;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(EnableCustomId));
            }
        }

        public bool EnableCustomId { get; set; }

        public string CustomOid { get; set; }

        public string Community { get; set; }
        public string AuthoritativeEngineId { get; set; }
        public string UserName { get; set; }
        public bool IsAuthPswSet { get; set; }
        public string AuthenticationPassword { get; set; }
        public List<string> AuthenticationProtocols { get; set; } = new List<string>() { @"Md5", @"Sha", @"Sha256", @"Sha384", @"Sha512" };
        public string SelectedAuthenticationProtocol { get; set; }
        public bool IsPrivPswSet { get; set; }
        public string PrivacyPassword { get; set; }
        public List<string> PrivacyProtocols { get; set; } = new List<string>()
        {
            @"None", @"Des", @"TripleDes", @"Aes128", @"Aes192", @"Aes256"
        };
        public string SelectedPrivacyProtocol { get; set; }
        public string TrapReceiverAddress { get; set; }
        public int TrapReceiverPort { get; set; }

        public List<string> Languages { get; set; } = new List<string>() { @"en-US", @"ru-RU" };
        public string SelectedLanguage { get; set; }


        public SnmpViewModel(SnmpService snmpService)
        {
            _snmpService = snmpService;

            LoadSnmpSettings();
        }
      
     private void LoadSnmpSettings()
        {
            var json = File.ReadAllText(@"../ini/snmp-settings.json");
            var snmp = JsonSerializer.Deserialize<SnmpNewSettings>(json);

            SelectedSnmpVersion = snmp.SnmpVersion;
            UseIitOid = snmp.UseIitOid;
            CustomOid = snmp.CustomOid;
            Community = snmp.Community;
            AuthoritativeEngineId = snmp.AuthoritativeEngineId;
            UserName = snmp.UserName;
            IsAuthPswSet = snmp.IsAuthPswSet;
            AuthenticationPassword = snmp.AuthenticationPassword;
            SelectedAuthenticationProtocol = snmp.AuthenticationProtocol;
            IsPrivPswSet = snmp.IsPrivPswSet;
            PrivacyPassword = snmp.PrivacyPassword;
            SelectedPrivacyProtocol = snmp.PrivacyProtocol;
            TrapReceiverAddress = snmp.TrapReceiverAddress;
            TrapReceiverPort = snmp.TrapReceiverPort;
            SelectedLanguage = snmp.SnmpLanguage;
        }

        public void SaveAndTest()
        {
            var snmpNewSettings = new SnmpNewSettings()
            {
                SnmpVersion = SelectedSnmpVersion,
                UseIitOid = UseIitOid,
                CustomOid = CustomOid,
                Community = Community,
                AuthoritativeEngineId = AuthoritativeEngineId,
                UserName = UserName,
                IsAuthPswSet = IsAuthPswSet,
                AuthenticationPassword = AuthenticationPassword,
                AuthenticationProtocol = SelectedAuthenticationProtocol,
                IsPrivPswSet = IsPrivPswSet,
                PrivacyPassword = PrivacyPassword,
                PrivacyProtocol = SelectedPrivacyProtocol,
                TrapReceiverAddress = TrapReceiverAddress,
                TrapReceiverPort = TrapReceiverPort,
                SnmpLanguage = SelectedLanguage
            };

            var json = JsonSerializer.Serialize(snmpNewSettings);
            File.WriteAllText(@"../ini/snmp-settings.json", json);

            var message = SelectedLanguage == "en-US" ? "Test string" : "Тестовая строка";
            var payload = new Dictionary<FtTrapProperty, string>()
            {
                { FtTrapProperty.TestString, message },
                { FtTrapProperty.EventRegistrationTime, DateTime.Now.ToString(CultureInfo.InvariantCulture) },
                { FtTrapProperty.TestInt, 123.ToString() },
                { FtTrapProperty.TestDouble, 3.1415926.ToString(CultureInfo.InvariantCulture) }
            };
            _snmpService.SendSnmpTrap(snmpNewSettings, FtTrapType.TestTrap, payload);
        }
    }
}
