﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public class TrapReceiver
    {
        private readonly IMyLog _logFile;
        private readonly OltTrapExecutor _oltTrapExecutor;

        public TrapReceiver(IMyLog logFile, OltTrapExecutor oltTrapExecutor)
        {
            _logFile = logFile;
            _oltTrapExecutor = oltTrapExecutor;
        }

        // http://snmpsharpnet.com/index.php/receive-snmp-version-1-and-2c-trap-notifications/
        public async void Start()
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
                    try
                    {
                        var inLen = socket.ReceiveFrom(inData, ref ipEndPoint);
                        await ProcessData(inData, inLen, ipEndPoint);
                    }
                    catch (Exception ex)
                    {
                        _logFile.AppendLine($"Exception {ex.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Failed to start listen to port 162. {e.Message}");
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private async Task ProcessData(byte[] inData, int inLen, EndPoint endPoint)
        {
            if (inLen > 0)
            {
                // Check protocol version int
                int ver = SnmpPacket.GetProtocolVersion(inData, inLen);
                if (ver == (int)SnmpVersion.Ver1)
                {
                    _logFile.AppendLine($"** SNMP Version 1 TRAP received from {endPoint}:");
                    SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
                    pkt.decode(inData, inLen);
                    LogSnmpVersion1TrapPacket(pkt);
                }
                else
                {
                    _logFile.AppendLine($"** SNMP Version 2 TRAP received from {endPoint}:");
                    SnmpV2Packet pkt = new SnmpV2Packet();
                    pkt.decode(inData, inLen);
                    LogSnmpVersion2TrapPacket(pkt); // Hide after debugging
                    await _oltTrapExecutor.Process(pkt, endPoint);
                }
            }
            else
            {
                if (inLen == 0)
                    _logFile.AppendLine("Zero length packet received.");
            }
        }

        private void LogSnmpVersion1TrapPacket(SnmpV1TrapPacket pkt)
        {
            _logFile.AppendLine($"*** Trap generic: {pkt.Pdu.Generic}");
            _logFile.AppendLine($"*** Trap specific: {pkt.Pdu.Specific}");
            _logFile.AppendLine($"*** Agent address: {pkt.Pdu.AgentAddress}");
            _logFile.AppendLine($"*** Timestamp: {pkt.Pdu.TimeStamp}");
            _logFile.AppendLine($"*** VarBind count: {pkt.Pdu.VbList.Count}");
            _logFile.AppendLine($"*** VarBind content:");
            foreach (Vb v in pkt.Pdu.VbList)
            {
                _logFile.AppendLine($"**** {v.Oid} {SnmpConstants.GetTypeName(v.Value.Type)}: {v.Value}");
            }

            _logFile.AppendLine($"** End of SNMP Version 1 TRAP data.");
        }

        private void LogSnmpVersion2TrapPacket(SnmpV2Packet pkt)
        {
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
