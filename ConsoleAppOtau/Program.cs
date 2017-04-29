using System;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.Utils35.IniFile;

namespace ConsoleAppOtau
{
    class Program
    {
        private static Logger35 _rtuLogger35;
        private static IniFile _iniFile35;
        static void Main()
        {
            _rtuLogger35 = new Logger35();
            _rtuLogger35.AssignFile("rtu.log");
            Console.WriteLine("see rtu.log");

            _iniFile35 = new IniFile();
            _iniFile35.AssignFile("rtu.ini");

            var otauAddress = _iniFile35.Read(IniSection.General, IniKey.OtauIp, "192.168.96.52");
            var otauPort = _iniFile35.Read(IniSection.General, IniKey.OtauPort, 23);
            var netAddress = new NetAddress() { Ip4Address = otauAddress, Port = otauPort };

            _rtuLogger35.AppendLine($"Otau {netAddress.ToStringA()} initialization started");
            var ch = new Charon(netAddress, _rtuLogger35, CharonLogLevel.TransmissionCommands);
            if (ch.Initialize())
                _rtuLogger35.AppendLine($"Otau initialization successful: Main charon {ch.Serial} has {ch.OwnPortCount}/{ch.FullPortCount} ports");
            else
                _rtuLogger35.AppendLine($"charon {ch.NetAddress.ToStringA()} initialization failed");

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
