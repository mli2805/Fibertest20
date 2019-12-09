using System;
using System.Globalization;
using System.Linq;
using System.Text;
using SnmpSharpNet;

namespace Iit.Fibertest.UtilsLib
{
    public class SnmpAgent
    {
        private readonly IniFile _iniFile;
        private DateTime startTime;

        private int snmpTrapVersion;
        private string snmpReceiverAddress;
        private int snmpReceiverPort;
        private string snmpAgentIp;
        private string snmpCommunity;
        private string snmpEncoding;

        private string enterpriseOid;

        public SnmpAgent(IniFile iniFile)
        {
            _iniFile = iniFile;
            Initialize();
        }

        private void Initialize()
        {
            startTime = DateTime.Now;
            snmpTrapVersion = 1; // for future purposes

            snmpReceiverAddress = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverIp, "192.168.96.21");
            snmpReceiverPort = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverPort, 162);

            var localIp = LocalAddressResearcher.GetAllLocalAddresses().FirstOrDefault() ?? "127.0.0.1";
            snmpAgentIp = _iniFile.Read(IniSection.Snmp, IniKey.SnmpAgentIp, localIp);

            snmpCommunity = _iniFile.Read(IniSection.Snmp, IniKey.SnmpCommunity, "IIT");
            snmpEncoding = _iniFile.Read(IniSection.Snmp, IniKey.SnmpEncoding, "windows1251");

            enterpriseOid = _iniFile.Read(IniSection.Snmp, IniKey.EnterpriseOid, "1.3.6.1.4.1.36220");
        }

        public void SendTestTrap()
        {
            var trapData = CreateTestTrapData();
            if (snmpTrapVersion == 1)
                SendSnmpV1Trap(trapData, 777);
        }

        private void SendSnmpV1Trap(VbCollection trapData, int trapType)
        {
            TrapAgent trapAgent = new TrapAgent();
            trapAgent.SendV1Trap(new IpAddress(snmpReceiverAddress),
                snmpReceiverPort,
                snmpCommunity,
                new Oid(enterpriseOid),
                new IpAddress(snmpAgentIp),
                6,
                trapType, // my trap type 
                (uint)(DateTime.Now - startTime).TotalSeconds * 10, // system UpTime in 0,1sec
                trapData);
        }

        private VbCollection CreateTestTrapData()
        {
            var trapData = new VbCollection();

            trapData.Add(new Oid(enterpriseOid + ".0"),
                new OctetString(
                    EncodeString("Test string with Русский язык.",
                    snmpEncoding)));
            trapData.Add(new Oid(enterpriseOid + ".1"), new Integer32(412));
            var doubleValue = 43.0319;
            trapData.Add(new Oid(enterpriseOid + ".2"), new OctetString(doubleValue.ToString(CultureInfo.CurrentUICulture)));
            return trapData;
        }

        private byte[] EncodeString(string str, string encondingName)
        {
            switch (encondingName.ToLower())
            {
                case "unicode":
                    var unicodeEncoding = new UnicodeEncoding();
                    return unicodeEncoding.GetBytes(str);
                case "windows1251":
                    var win1251Encoding = Encoding.GetEncoding("windows-1251");
                    return win1251Encoding.GetBytes(str);
                case "utf8":
                default:
                    var utf8Encoding = new UTF8Encoding();
                    return utf8Encoding.GetBytes(str);
            }
        }
    }
}
