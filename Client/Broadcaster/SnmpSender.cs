using System;
using System.Globalization;
using System.Text;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;

namespace Broadcaster
{
    public class SnmpSender
    {
        private readonly IniFile _iniFile;
        private DateTime startTime;

        private int snmpTrapVersion;
        private string snmpReceiverAddress;
        private int snmpReceiverPort;
        private string snmpCommunity;

        private string enterpriseOid;

        public SnmpSender(IniFile iniFile)
        {
            _iniFile = iniFile;
            Initialize();
        }

        private void Initialize()
        {
            startTime = DateTime.Now;
            snmpTrapVersion = _iniFile.Read(IniSection.Snmp, IniKey.SnmpTrapVersion, 1);

            snmpReceiverAddress = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverIp, "localhost");
            snmpReceiverPort = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverPort, 162);
            snmpCommunity = _iniFile.Read(IniSection.Snmp, IniKey.SnmpCommunity, "iit");

            enterpriseOid = _iniFile.Read(IniSection.Snmp, IniKey.EnterpriseOid, "1.3.6.1.4.1.36220");
        }

        public void SendTestTrap()
        {
            var trapData = CreateTestTrapData();
            if (snmpTrapVersion == 1)
                SendSnmpV1Trap(trapData);
        }

        private void SendSnmpV1Trap(VbCollection trapData)
        {
            TrapAgent trapAgent = new TrapAgent();
            trapAgent.SendV1Trap(new IpAddress(snmpReceiverAddress), 
                snmpReceiverPort, 
                snmpCommunity,
                new Oid(enterpriseOid), 
                new IpAddress("192.168.96.17"), 
                6, 
                777, // my trap type 
                (uint)(DateTime.Now - startTime).TotalSeconds * 10, 
                trapData);
        }

        private VbCollection CreateTestTrapData()
        {
            var trapData = new VbCollection();
            byte[] bytes = Encoding.Default.GetBytes("Test string on русский язык");
            var stringValue = Encoding.UTF8.GetString(bytes);
            trapData.Add(new Oid(enterpriseOid+".0"), new OctetString(stringValue));
            trapData.Add(new Oid(enterpriseOid+".1"), new Integer32(412));
            var doubleValue = 43.0319;
            trapData.Add(new Oid(enterpriseOid+".2"), new OctetString(doubleValue.ToString(CultureInfo.CurrentUICulture)));
            return trapData;
        }
    }
}
