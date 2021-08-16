using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public class TrapExecutor
    {
        private readonly IMyLog _logFile;

        public TrapExecutor(IMyLog logFile)
        {
            _logFile = logFile;
        }

        // http://snmpsharpnet.com/index.php/receive-snmp-version-1-and-2c-trap-notifications/
        public void Start()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"Trap listener thread. Process {pid}, thread {tid}");

            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                EndPoint ep = new IPEndPoint(IPAddress.Any, 162);
                socket.Bind(ep);
                // Disable timeout processing. Just block until packet is received
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);


                while (true)
                {
                    byte[] inData = new byte[16 * 1024];
                    EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int inLen;
                    try
                    {
                        inLen = socket.ReceiveFrom(inData, ref ipEndPoint);
                    }
                    catch (Exception ex)
                    {
                        _logFile.AppendLine($"Exception {ex.Message}");
                        inLen = -1;
                    }
                    if (inLen > 0)
                    {
                        // Check protocol version int
                        int ver = SnmpPacket.GetProtocolVersion(inData, inLen);
                        if (ver == (int)SnmpVersion.Ver1)
                            ParseSnmpVersion1TrapPacket(inData, inLen, ipEndPoint);
                        else
                            ParseSnmpVersion2TrapPacket(inData, inLen, ipEndPoint);
                    }
                    else
                    {
                        if (inLen == 0)
                            _logFile.AppendLine("Zero length packet received.");
                    }
                }

            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Failed to establish connection. {e.Message}");
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void ParseSnmpVersion1TrapPacket(byte[] inData, int inLen, EndPoint endPoint)
        {
            SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
            pkt.decode(inData, inLen);
            _logFile.AppendLine($"** SNMP Version 1 TRAP received from {endPoint}:");
            _logFile.AppendLine($"*** Trap generic: {pkt.Pdu.Generic}");
            _logFile.AppendLine($"*** Trap specific: {pkt.Pdu.Specific}");
            _logFile.AppendLine($"*** Agent address: {pkt.Pdu.AgentAddress}");
            _logFile.AppendLine($"*** Timestamp: {pkt.Pdu.TimeStamp.ToString()}");
            _logFile.AppendLine($"*** VarBind count: {pkt.Pdu.VbList.Count}");
            _logFile.AppendLine($"*** VarBind content:");
            foreach (Vb v in pkt.Pdu.VbList)
            {
                _logFile.AppendLine($"**** {v.Oid} {SnmpConstants.GetTypeName(v.Value.Type)}: {v.Value}");
            }

            _logFile.AppendLine($"** End of SNMP Version 1 TRAP data.");
        }

        private void ParseSnmpVersion2TrapPacket(byte[] inData, int inLen, EndPoint ipEndPoint)
        {
            SnmpV2Packet pkt = new SnmpV2Packet();
            pkt.decode(inData, inLen);
            _logFile.AppendLine($"** SNMP Version 2 TRAP received from {ipEndPoint}:");
            if (pkt.Pdu.Type != PduType.V2Trap)
            {
                _logFile.AppendLine($"*** NOT an SNMPv2 trap ****");
            }
            else
            {
                _logFile.AppendLine($"*** Community: {pkt.Community}"); 
                _logFile.AppendLine($"*** System Up Time: {new TimeSpan(pkt.Pdu.TrapSysUpTime * 100000)}");
                _logFile.AppendLine($"*** VarBind count: {pkt.Pdu.VbList.Count}");
                _logFile.AppendLine($"*** VarBind content:");
                foreach (Vb v in pkt.Pdu.VbList)
                {
                    _logFile.AppendLine($"**** {v.Oid} {SnmpConstants.GetTypeName(v.Value.Type)}: {v.Value}");
                }

                _logFile.AppendLine($"** End of SNMP Version 2 TRAP data.");
            }
        }
 }
}
