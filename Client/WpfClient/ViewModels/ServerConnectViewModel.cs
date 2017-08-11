using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.Client
{
    public class ServerConnectViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private string _message;
        public NetAddressTestViewModel ServerConnectionTestViewModel { get; set; }

        public string Message
        {
            get { return _message; }
            set
            {
                if (value == _message) return;
                _message = value;
                NotifyOfPropertyChange();
            }
        }

        public ServerConnectViewModel(IniFile iniFile)
        {
            _iniFile = iniFile;
            ServerConnectionTestViewModel = new NetAddressTestViewModel(_iniFile.Read(IniSection.ServerMainAddress));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Server;
            Message = Resources.SID_Enter_DataCenter_Server_address;
        }

        public void Save()
        {
            var serverAddress = ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress();
            _iniFile.Write(serverAddress, IniSection.ServerMainAddress);
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}