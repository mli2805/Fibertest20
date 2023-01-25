﻿using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Utils471;

namespace Broadcaster
{
    public class SnmpViewModel : PropertyChangedBase
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private int _snmpTrapVersion;
        public string SnmpManagerIp { get; set; }
        public int SnmpManagerPort { get; set; }
        public string SnmpCommunity { get; set; }

        public List<string> SnmpEncodings { get; set; } = new List<string>() { "unicode (utf16)", "utf8", "windows1251" };

        public string SelectedSnmpEncoding { get; set; }
        public string EnterpriseOid { get; set; }


        public SnmpViewModel(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;

            LoadSnmpSets();
            SelectedSnmpEncoding = SnmpEncodings[2];
        }
      
        public void SendV1TestTrap()
        {
            // save all user's input into ini-file: snmpAgent will read them from ini-file
            SaveInputs();

            var snmpAgent = new SnmpAgent(_iniFile, _logFile);
            var unused = snmpAgent.SendTestTrap();
        }

    
        private void LoadSnmpSets()
        {
            _snmpTrapVersion = _iniFile.Read(IniSection.Snmp, IniKey.SnmpTrapVersion, 1);
            SnmpManagerIp = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverIp, "192.168.96.21");
            SnmpManagerPort = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverPort, 162);
            SnmpCommunity = _iniFile.Read(IniSection.Snmp, IniKey.SnmpCommunity, "IIT");
            SelectedSnmpEncoding = _iniFile.Read(IniSection.Snmp, IniKey.SnmpEncoding, "windows1251");
            EnterpriseOid = _iniFile.Read(IniSection.Snmp, IniKey.EnterpriseOid, "1.3.6.1.4.1.36220");
        }

        private void SaveInputs()
        {
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpTrapVersion, _snmpTrapVersion);

            _iniFile.Write(IniSection.Snmp, IniKey.SnmpReceiverIp, SnmpManagerIp);
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpReceiverPort, SnmpManagerPort);
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpCommunity, SnmpCommunity);
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpEncoding, SelectedSnmpEncoding);

            _iniFile.Write(IniSection.Snmp, IniKey.EnterpriseOid, EnterpriseOid);
        }
    }
}
