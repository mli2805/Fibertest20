using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Iit.Fibertest.Dto;
using SnmpSharpNet;

namespace Iit.Fibertest.UtilsLib
{
    // should be the same as in MIB file
    public enum SnmpTrapType
    {
        MeasurementAsSnmp = 100,
        RtuNetworkEventAsSnmp = 200,
        BopNetworkEventAsSnmp = 300,
        TestTrap = 777,
    }

    // should be the same as in MIB file
    public enum SnmpProperty
    {
        EventId = 0,
        EventRegistrationTime,
        RtuTitle,
        TraceTitle,

        RtuMainChannel = 10,
        RtuReserveChannel = 11,

        BopTitle = 20,
        BopState,

        TraceState = 30,
        AccidentNodeTitle,
        AccidentType,
        AccidentToRtuDistanceKm,
        AccidentGps,

        LeftNodeTitle = 40,
        LeftNodeGps,
        LeftNodeToRtuDistanceKm,

        RightNodeTitle = 50,
        RightNodeGps,
        RightNodeToRtuDistanceKm,

        TestString = 700,
        TestInt,
        TestDouble,
    }

    public class SnmpAgent
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private DateTime startTime;

        private int snmpTrapVersion;
        private string snmpReceiverAddress;
        private int snmpReceiverPort;
        private string snmpAgentIp;
        private string snmpCommunity;
        private string snmpEncoding;

        private string enterpriseOid;

        public SnmpAgent(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            startTime = DateTime.Now;
            Initialize();
        }

        private void Initialize()
        {
            snmpTrapVersion = 1; // for future purposes

            snmpReceiverAddress = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverIp, "192.168.96.21");
            snmpReceiverPort = _iniFile.Read(IniSection.Snmp, IniKey.SnmpReceiverPort, 162);

            var localIp = LocalAddressResearcher.GetAllLocalAddresses().FirstOrDefault() ?? "127.0.0.1";
            snmpAgentIp = _iniFile.Read(IniSection.Snmp, IniKey.SnmpAgentIp, localIp);

            snmpCommunity = _iniFile.Read(IniSection.Snmp, IniKey.SnmpCommunity, "IIT");
            snmpEncoding = _iniFile.Read(IniSection.Snmp, IniKey.SnmpEncoding, "windows1251");

            enterpriseOid = _iniFile.Read(IniSection.Snmp, IniKey.EnterpriseOid, "1.3.6.1.4.1.36220");
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
            if (snmpTrapVersion == 1)
                return SendSnmpV1Trap(trapData, SnmpTrapType.TestTrap);
            return false;
        }

        private bool SendSnmpV1Trap(VbCollection trapData, SnmpTrapType trapType)
        {
            try
            {
                TrapAgent trapAgent = new TrapAgent();
                trapAgent.SendV1Trap(new IpAddress(snmpReceiverAddress),
                    snmpReceiverPort,
                    snmpCommunity,
                    new Oid(enterpriseOid),
                    new IpAddress(snmpAgentIp),
                    6,
                    (int)trapType, // my trap type 
                    (uint)(DateTime.Now - startTime).TotalSeconds * 10, // system UpTime in 0,1sec
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

        public bool SentRealTrap(List<KeyValuePair<SnmpProperty, string>> data, SnmpTrapType trapType)
        {
            var trapData = new VbCollection();
            foreach (KeyValuePair<SnmpProperty, string> pair in data)
            {
                trapData.Add(new Oid(enterpriseOid + "." + (int)pair.Key),
                    new OctetString(EncodeString(pair.Value, snmpEncoding)));
            }
            return SendSnmpV1Trap(trapData, trapType);
        }

        private VbCollection CreateTestTrapData()
        {
            var trapData = new VbCollection();

            trapData.Add(new Oid(enterpriseOid + "." + (int)SnmpProperty.TestString),
                new OctetString(EncodeString("Test string with Русский язык.",snmpEncoding)));
            trapData.Add(new Oid(enterpriseOid + "." + (int)SnmpProperty.EventRegistrationTime), 
                new OctetString(DateTime.Now.ToString("G")));
            trapData.Add(new Oid(enterpriseOid + "." + (int)SnmpProperty.TestInt), new Integer32(412));
            var doubleValue = 43.0319;
            trapData.Add(new Oid(enterpriseOid + "." + (int)SnmpProperty.TestDouble), 
                new OctetString(doubleValue.ToString(CultureInfo.CurrentUICulture)));
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
                // ReSharper disable once RedundantCaseLabel
                case "utf8":
                default:
                    var utf8Encoding = new UTF8Encoding();
                    return utf8Encoding.GetBytes(str);
            }
        }
    }
}
