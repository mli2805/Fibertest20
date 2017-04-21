using System;
using DirectCharonLibrary;
using Iit.Fibertest.Utils35;

namespace ConsoleAppOtau
{
    class Program
    {
        private static Logger35 _rtuLogger35;
        static void Main()
        {
            Console.WriteLine("rtu.log");
            _rtuLogger35 = new Logger35().AssignFile("rtu.log");
//          const string serverIp = "192.168.88.101";
            const string serverIp = "192.168.96.52";
//          const string serverIp = "192.168.96.57";
//          const string serverIp = "172.16.4.10";
//          const int tcpPort = 11834;

            const int tcpPort = 23;
            _rtuLogger35.AppendLine("Otau initialization started");
            var ch = new Charon(new NetAddress() { Ip4Address = serverIp, Port = tcpPort }, _rtuLogger35);
            if (ch.Initialize())
                _rtuLogger35.AppendLine($"charon {ch.Serial} has {ch.OwnPortCount} ports");

            //reinit
//            if (ch.Initialize())
//                _rtuLogger.AppendLine($"charon {ch.Serial} has {ch.OwnPortCount} ports");

            var activePort = ch.GetExtendedActivePort();
            if (activePort != -1)
                _rtuLogger35.AppendLine($"{ch.NetAddress.Ip4Address}:{ch.NetAddress.Port} active port {activePort}");
            else
                _rtuLogger35.AppendLine("some error");

            var newActivePort = ch.SetExtendedActivePort(14);
            if (newActivePort == -1)
            {
                _rtuLogger35.AppendLine(ch.LastErrorMessage);
                newActivePort = ch.GetExtendedActivePort();
            }
            _rtuLogger35.AppendLine($"New active port {newActivePort}");


            if (ch.DetachOtauFromPort(2))
                _rtuLogger35.AppendLine($"detached successfully");
            else _rtuLogger35.AppendLine($"{ch.LastErrorMessage}");

            if (ch.AttachOtauToPort(new NetAddress("192.168.96.57", 11834) , 2))
                _rtuLogger35.AppendLine($"attached successfully");
            else _rtuLogger35.AppendLine($"{ch.LastErrorMessage}");

            Console.WriteLine("Done. Press Enter.");
            Console.ReadLine();
        }
    }
}
