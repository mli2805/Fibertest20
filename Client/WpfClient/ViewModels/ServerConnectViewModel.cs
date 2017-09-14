using System;
using Caliburn.Micro;
using Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

namespace Iit.Fibertest.Client
{
    public class ServerConnectViewModel : Screen
    {
        private readonly IniFile _clientIni;
        private string _message;
        private DoubleAddress _dcServiceAddresses;
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

        public ServerConnectViewModel(Guid clientId, IniFile iniFile, LogFile logFile)
        {
            _clientIni = iniFile;
            _dcServiceAddresses = _clientIni.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);

            ServerConnectionTestViewModel = new NetAddressTestViewModel((NetAddress)_dcServiceAddresses.Main.Clone(), _clientIni, logFile, clientId);
            ServerConnectionTestViewModel.PropertyChanged += ServerConnectionTestViewModel_PropertyChanged;
        }

        private void ServerConnectionTestViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Result")
            {
                Message = ServerConnectionTestViewModel.Result ? Resources.SID_Connection_established_successfully_ : Resources.SID_Cant_establish_connection_;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Server;
            Message = Resources.SID_Enter_DataCenter_Server_address;
        }

        public void Save()
        {
            _dcServiceAddresses.Main = (NetAddress)ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress().Clone();
            _clientIni.WriteServerAddresses(_dcServiceAddresses);

            _dcServiceAddresses = _clientIni.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            IoC.Get<C2DWcfManager>().ChangeServerAddresses(_dcServiceAddresses);

            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}