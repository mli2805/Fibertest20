using System;
using System.Collections.Generic;
using SnmpSharpNet;

namespace Iit.Fibertest.UtilsLib
{
    public class SnmpHuaweiAgent
    {
        private readonly SnmpAgent _snmpAgent;
        private const string HuaweiOid = "1.3.6.1.4.1.2011.2.248";

        public SnmpHuaweiAgent(SnmpAgent snmpAgent)
        {
            _snmpAgent = snmpAgent;
        }

        public bool SendV2CPonTestTrap(DateTime systemStartTime)
        {
            var trapData = CreateOltTrap();
            return _snmpAgent.SendSnmpV2CTrap(trapData, systemStartTime, new Oid(HuaweiOid));
        }



        private VbCollection CreateOltTrap()
        {
            var data = new List<Tuple<string, string, SnmpV2CDataType>>();

            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "0", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "4", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "3", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "192.168.96.59", SnmpV2CDataType.IpAddress));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "416", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "0", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "0", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "5", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "0", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "00000000", SnmpV2CDataType.OctetString));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "2", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, "1", SnmpV2CDataType.Integer32));
            data.Add(new Tuple<string, string, SnmpV2CDataType>(HuaweiOid, DateTime.Now.ToString("O"), SnmpV2CDataType.OctetString));

            return new VbCollection(VbCollectionFactory.CreateCollection(data));
        }
    }
}