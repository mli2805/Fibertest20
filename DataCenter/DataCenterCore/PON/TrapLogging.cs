using System;
using System.Net;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public static class TrapLogging
    {
        public static void LogSnmpVersion1TrapPacket(this IMyLog logFile, SnmpV1TrapPacket pkt, EndPoint endPoint, int messageLevel)
        {
            if (logFile.LogLevel < messageLevel) return;

            logFile.EmptyLine();
            logFile.AppendLine($"** SNMP Version 1 TRAP received from {endPoint}:");
            logFile.AppendLine($"*** Trap generic: {pkt.Pdu.Generic}");
            logFile.AppendLine($"*** Trap specific: {pkt.Pdu.Specific}");
            logFile.AppendLine($"*** Agent address: {pkt.Pdu.AgentAddress}");
            logFile.AppendLine($"*** Timestamp: {pkt.Pdu.TimeStamp}");
            logFile.AppendLine($"*** VarBind count: {pkt.Pdu.VbList.Count}");
            logFile.AppendLine($"*** VarBind content:");
            foreach (Vb v in pkt.Pdu.VbList)
            {
                logFile.AppendLine($"**** {v.Oid} {SnmpConstants.GetTypeName(v.Value.Type)}: {v.Value}");
            }

            logFile.AppendLine($"** End of SNMP Version 1 TRAP data.");
        }

        public static void LogSnmpVersion2TrapPacket(this IMyLog logFile, SnmpV2Packet pkt, EndPoint endPoint, int messageLevel)
        {
            if (logFile.LogLevel < messageLevel) return;
          
            if (pkt.Pdu.Type != PduType.V2Trap)
            {
                logFile.AppendLine($"*** NOT an SNMPv2 trap ****");
            }
            else
            {
                logFile.EmptyLine();
                logFile.AppendLine($"** SNMP Version 2 TRAP received from {endPoint}:");
                logFile.AppendLine($"*** Community: {pkt.Community}");
                logFile.AppendLine($"*** System Up Time: {new TimeSpan(pkt.Pdu.TrapSysUpTime * 100000)}");
                logFile.AppendLine($"*** VarBind count: {pkt.Pdu.VbList.Count}");
                logFile.AppendLine($"*** VarBind content:");
                foreach (Vb v in pkt.Pdu.VbList)
                {
                    logFile.AppendLine($"**** {v.Oid} {SnmpConstants.GetTypeName(v.Value.Type)}: {v.Value}");
                }

                logFile.AppendLine($"** End of SNMP Version 2 TRAP data.");
            }
        }
    }
}