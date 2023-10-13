using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;

namespace Utils471
{
    public class SnmpAgent
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private int _snmpTrapVersion;
        private string _snmpReceiverAddress;
        private int _snmpReceiverPort;
        private string _snmpAgentIp;
        private string _snmpCommunity;
        private string _snmpEncoding;

        private string _enterpriseOid;

        public SnmpAgent(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;

            Initialize();
        }

        private void Initialize()
        {
            _snmpTrapVersion = 1; // for future purposes

            _snmpReceiverAddress = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverIp, "192.168.96.21");
            _snmpReceiverPort = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverPort, 162);

            var localIp = LocalAddressResearcher.GetAllLocalAddresses().FirstOrDefault() ?? "127.0.0.1";
            _snmpAgentIp = _iniFile.Read(IniSection.Snmp, IniKey.SnmpAgentIp, localIp);

            _snmpCommunity = _iniFile.Read(IniSection.Snmp, IniKey.SnmpCommunity, "IIT");
            _snmpEncoding = _iniFile.Read(IniSection.Snmp, IniKey.SnmpEncoding, "windows1251");

            _enterpriseOid = _iniFile.Read(IniSection.Snmp, IniKey.EnterpriseOid, "1.3.6.1.4.1.36220");
        }

        public void SaveSnmpSettings(SnmpSettingsDto dto)
        {
            _iniFile.Write(IniSection.Snmp, IniKey.IsSnmpOn, dto.IsSnmpOn);

            _iniFile.Write(IniSection.Snmp, IniKey.SnmpReceiverIp, dto.SnmpReceiverIp);
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpReceiverPort, dto.SnmpReceiverPort);
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpCommunity, dto.SnmpCommunity);
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpAgentIp, dto.SnmpAgentIp);
            _iniFile.Write(IniSection.Snmp, IniKey.SnmpEncoding, dto.SnmpEncoding);

            _iniFile.Write(IniSection.Snmp, IniKey.EnterpriseOid, dto.EnterpriseOid);

            Initialize();
        }

        public bool SendTestTrap()
        {
            var trapData = CreateTestTrapData();
            if (_snmpTrapVersion == 1)
                return SendSnmpV1Trap(trapData, FtTrapType.TestTrap);
            return false;
        }

        private bool SendSnmpV1Trap(VbCollection trapData, FtTrapType trapType)
        {
            try
            {
                TrapAgent trapAgent = new TrapAgent();
                trapAgent.SendV1Trap(new IpAddress(_snmpReceiverAddress),
                    _snmpReceiverPort,
                    _snmpCommunity,
                    new Oid(_enterpriseOid),
                    new IpAddress(_snmpAgentIp),
                    6,
                    (int)trapType, // my trap type 
                    12345678, // system UpTime in 0,1sec
                    trapData);
                _logFile.AppendLine("SendSnmpV1Trap sent.");
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"SendSnmpV1Trap: {e.Message}");
                return false;
            }
        }

      

        public bool SendSnmpV2CTrap(VbCollection trapData, uint upTime, Oid trapObjOid)
        {
            try
            {
                TrapAgent trapAgent = new TrapAgent();
                trapAgent.SendV2Trap(new IpAddress(_snmpReceiverAddress),
                    _snmpReceiverPort,
                    "public",
                    upTime,
                    trapObjOid,
                    trapData);
                _logFile.AppendLine("SendSnmpV2Trap sent.");
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"SendSnmpV2Trap: {e.Message}");
                return false;
            }
        }

        public bool SendRealTrap(List<KeyValuePair<FtTrapProperty, string>> data, FtTrapType trapType)
        {
            var trapData = new VbCollection();
            foreach (KeyValuePair<FtTrapProperty, string> pair in data)
            {
                trapData.Add(new Oid(_enterpriseOid + "." + (int)pair.Key),
                    new OctetString(EncodeString(pair.Value, _snmpEncoding)));
            }
            return SendSnmpV1Trap(trapData, trapType);
        }

        private VbCollection CreateTestTrapData()
        {
            var trapData = new VbCollection
            {
                // with encoding
                {
                    new Oid(_enterpriseOid + "." + (int)FtTrapProperty.TestString),
                    new OctetString(EncodeString("Test string with Русский язык.", _snmpEncoding))
                },
                // {
                //     new Oid(_enterpriseOid + "." + (int)FtTrapProperty.TestString),
                //     new OctetString(EncodeString("Test string with", _snmpEncoding))
                // },
                //
                // // NO encoding, just string into constructor of OctetString
                // { new Oid(_enterpriseOid + "." + (int)FtTrapProperty.TestString), new OctetString("Test string with Русский язык.") },
                // { new Oid(_enterpriseOid + "." + (int)FtTrapProperty.TestString), new OctetString("Test string with") },
                
                { new Oid(_enterpriseOid + "." + (int)FtTrapProperty.EventRegistrationTime), new OctetString(DateTime.Now.ToString("G")) },
                { new Oid(_enterpriseOid + "." + (int)FtTrapProperty.TestInt), new Integer32(412) },
                { new Oid(_enterpriseOid + "." + (int)FtTrapProperty.TestDouble), new OctetString(43.0319.ToString(CultureInfo.CurrentUICulture)) }
            };

            return trapData;
        }

        // можно строку не кодировать
        // если строку передать в конструктор OctetString это тоже самое что закодировать в utf8 и передать массив байт
        private byte[] EncodeString(string str, string encodingName)
        {
            switch (encodingName.ToLower())
            {
                case "unicode":
                    var unicodeEncoding = new UnicodeEncoding();
                    return unicodeEncoding.GetBytes(str);
                case "windows1251":
                    var win1251Encoding = Encoding.GetEncoding("windows-1251");
                    return win1251Encoding.GetBytes(str);
                // ReSharper disable once RedundantCaseLabel
                case "utf8":
                default:
                    var utf8Encoding = new UTF8Encoding();
                    return utf8Encoding.GetBytes(str);
            }
        }
    }
}
