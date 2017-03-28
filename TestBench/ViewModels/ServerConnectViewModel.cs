using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class ServerConnectViewModel : Screen
    {
        private readonly IniFile _iniFile;
        public NetAddressTestViewModel ServerConnectionTestViewModel;

        public ServerConnectViewModel(IniFile iniFile)
        {
            _iniFile = iniFile;
            _iniFile.Read("General", "ServerIp", "192.168.96.21");
        }
    }
}
