using Caliburn.Micro;
using Dto;
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
            var serverIp = iniFile.Read(IniSection.DataCenter, IniKey.MainAddress, @"192.168.96.179");
            var serverPort = iniFile.Read(IniSection.DataCenter, IniKey.ServerTcpPort, 11840);

            ServerConnectionTestViewModel = new NetAddressTestViewModel(new NetAddress(serverIp, serverPort));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Server;
            Message = Resources.SID_Enter_DataCenter_Server_address;
        }

        public void Save()
        {
            var serverAddress = ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress();
            _iniFile.Write(IniSection.DataCenter, IniKey.MainAddress, serverAddress.Ip4Address);
            _iniFile.Write(IniSection.DataCenter, IniKey.ServerTcpPort, serverAddress.Port);
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}