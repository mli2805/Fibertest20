using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Lextm.SharpSnmpLib;

namespace Utils471
{
    public class HuaweiSnmpService
    {
        private const string HuaweiOid = "1.3.6.1.4.1.2011.2.248";
        // MGTS huawei
        // private const string HuaweiOid2 = "1.3.6.1.4.1.2011.2.80.8";
        
        public bool SendV2CPonTestTrap(SnmpNewSettings snmpNewSettings, 
            DateTime systemStartTime, int slotPosition, int interfaceNumber, int trapNumber)
        {
            var trapData = CreateOltTrapPayload(
                HuaweiOid,"192.168.96.59", slotPosition, interfaceNumber, trapNumber);
            var upTime = (uint)(DateTime.Now - systemStartTime).TotalSeconds * 100; // Huawei OLT sends UpTime in 0,1sec,

            var snmpService = new SnmpService();
            return snmpService.SendOltTrap(snmpNewSettings, 
                new ObjectIdentifier(HuaweiOid), upTime, trapData.ToList());
        }

        private IEnumerable<Variable> CreateOltTrapPayload(string huaweiOid, 
            string oltIp, int slotPosition, int interfaceNumber, int trapNumber)
        {
            yield return new Variable(new ObjectIdentifier(huaweiOid), new OctetString("0"));
            yield return new Variable(new ObjectIdentifier(huaweiOid), new OctetString("4"));
            yield return new Variable(new ObjectIdentifier(huaweiOid), new OctetString("3"));
            yield return new Variable(new ObjectIdentifier(huaweiOid), new OctetString(oltIp));
            yield return new Variable(new ObjectIdentifier(huaweiOid), new OctetString("416"));
            yield return new Variable(new ObjectIdentifier(huaweiOid), new OctetString("0"));
            yield return new Variable(new ObjectIdentifier(huaweiOid),
                new OctetString(slotPosition.ToString()));
            yield return new Variable(new ObjectIdentifier(huaweiOid),
                new OctetString(interfaceNumber.ToString()));
            yield return new Variable(new ObjectIdentifier(huaweiOid),
                new OctetString((trapNumber + 567000).ToString()));
            yield return new Variable(new ObjectIdentifier(huaweiOid), new OctetString("00000000"));
            yield return new Variable(new ObjectIdentifier(huaweiOid), new OctetString("2"));
            yield return new Variable(new ObjectIdentifier(huaweiOid), new OctetString("2"));
            yield return new Variable(new ObjectIdentifier(huaweiOid),
                new OctetString(DateTime.Now.ToString("O")));

        }
    }
}
