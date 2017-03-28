using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class ServerConnectViewModel : Screen
    {
        public NetAddressTestViewModel ServerConnectionTestViewModel;

        public ServerConnectViewModel(IniFile iniFile)
        {
            var serverIp = iniFile.Read(IniSection.General, IniKey.ServerIp, @"192.168.96.21");

            ServerConnectionTestViewModel = new NetAddressTestViewModel(new NetAddress(serverIp, 118331));
        }
    }
}
