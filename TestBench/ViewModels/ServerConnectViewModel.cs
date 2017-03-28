using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class ServerConnectViewModel : Screen
    {
        public NetAddressTestViewModel ServerConnectionTestViewModel { get; set; }

        public ServerConnectViewModel(IniFile iniFile)
        {
            var serverIp = iniFile.Read(IniSection.General, IniKey.ServerIp, @"192.168.96.21");

            ServerConnectionTestViewModel = new NetAddressTestViewModel(new NetAddress(serverIp, 11831));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Server;
        }

        public void Save()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
