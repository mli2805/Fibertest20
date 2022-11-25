using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public class SnmpTrapListener
    {
        private readonly IMyLog _logFile;
        private readonly OutOfTurnRequestBuilder _outOfTurnRequestBuilder;
        private readonly OutOfTurnData _outOfTurnData;
        private readonly TrapParser _trapParser;

        public SnmpTrapListener(IniFile iniFile, Model writeModel, OutOfTurnData outOfTurnData, ClientsCollection clientsCollection)
        {
            _logFile = new LogFile(iniFile, 20000);
            _logFile.AssignFile("trap.log");
            _outOfTurnData = outOfTurnData;

            _trapParser = new TrapParser(_logFile, writeModel);
            _outOfTurnRequestBuilder = new OutOfTurnRequestBuilder(_logFile, writeModel, clientsCollection.TrapConnectionId);
        }

        public void Start()
        {
            var thread = new Thread(Listen) { IsBackground = true };
            thread.Start();
        }


        // http://snmpsharpnet.com/index.php/receive-snmp-version-1-and-2c-trap-notifications/
        private async void Listen()
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
            await Task.Delay(1);
            if (inLen > 0)
            {
                // Check protocol version int
                int ver = SnmpPacket.GetProtocolVersion(inData, inLen);
                if (ver == (int)SnmpVersion.Ver1)
                {
                    SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
                    pkt.decode(inData, inLen);
                    _logFile.LogSnmpVersion1TrapPacket(pkt, endPoint, 3);
                }
                else
                {
                    SnmpV2Packet pkt = new SnmpV2Packet();
                    pkt.decode(inData, inLen);
                    _logFile.LogSnmpVersion2TrapPacket(pkt, endPoint, 3);
                    var parsedTrap = _trapParser.ParseTrap(pkt, endPoint);
                    if (parsedTrap == null) return;
                    var dto = _outOfTurnRequestBuilder.BuildDto(parsedTrap);
                    if (dto == null) return;

                    _outOfTurnData.AddNewRequest(dto, _logFile);
                }
            }
            else
            {
                if (inLen == 0)
                    _logFile.AppendLine("Zero length packet received.");
            }
        }
    }
}
