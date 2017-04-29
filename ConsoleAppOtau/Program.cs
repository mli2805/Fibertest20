using System;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Utils35;

namespace ConsoleAppOtau
{
    class Program
    {
        private static Logger35 _rtuLogger35;
        static void Main()
        {
            _rtuLogger35 = new Logger35();
            _rtuLogger35.AssignFile(@"c:\temp\charon.log"); // if filename is empty Console will be used

//          const string serverIp = "192.168.88.101";
            const string serverIp = "192.168.96.52";
//          const string serverIp = "192.168.96.57";
//          const string serverIp = "172.16.4.10";
//          const int tcpPort = 11834;

            const int tcpPort = 23;
            var netAddress = new NetAddress() { Ip4Address = serverIp, Port = tcpPort };
            _rtuLogger35.AppendLine($"Otau {netAddress.ToStringA()} initialization started");
            var ch = new Charon(netAddress, _rtuLogger35, CharonLogLevel.Off);
            if (ch.Initialize())
                _rtuLogger35.AppendLine($"Otau initialization successful: Main charon {ch.Serial} has {ch.OwnPortCount}/{ch.FullPortCount} ports");
            else
                _rtuLogger35.AppendLine($"charon {ch.NetAddress.ToStringA()} initialization failed");

            //reinit
            //            if (ch.Initialize())
            //                _rtuLogger.AppendLine($"charon {ch.Serial} has {ch.OwnPortCount} ports");

            _rtuLogger35.AppendLine("Otau get active port");
            var activePort = ch.GetExtendedActivePort();
            if (activePort != -1)
                _rtuLogger35.AppendLine($"Otau active port {activePort}");
            else
                _rtuLogger35.AppendLine("some error");

            _rtuLogger35.AppendLine("Otau set new active port");
            var newActivePort = ch.SetExtendedActivePort(14);
            if (newActivePort == -1)
            {
                _rtuLogger35.AppendLine(ch.LastErrorMessage);
                newActivePort = ch.GetExtendedActivePort();
            }
            _rtuLogger35.AppendLine($"New active port {newActivePort}");


            _rtuLogger35.AppendLine("Otau detach additional otau");
            if (ch.DetachOtauFromPort(2))
                _rtuLogger35.AppendLine($"detached successfully");
            else _rtuLogger35.AppendLine($"{ch.LastErrorMessage}");


            _rtuLogger35.AppendLine("Otau attach additional otau");
            if (ch.AttachOtauToPort(new NetAddress("192.168.96.57", 11834) , 2))
                _rtuLogger35.AppendLine($"attached successfully");
            else _rtuLogger35.AppendLine($"{ch.LastErrorMessage}");

            Console.WriteLine("Done. Press Enter.");
            Console.ReadLine();
        }
    }
}
