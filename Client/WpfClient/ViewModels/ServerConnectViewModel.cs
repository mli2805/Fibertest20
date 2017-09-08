using System;
using Caliburn.Micro;
using Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class ServerConnectViewModel : Screen
    {
        private readonly Guid _clientId;
        private readonly IniFile _clientIni;
        private readonly LogFile _logFile;
        private string _message;
        private readonly DoubleAddress _dcServiceAddresses;
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
            _clientId = clientId;
            _clientIni = iniFile;
            _logFile = logFile;
            _dcServiceAddresses = _clientIni.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);

            ServerConnectionTestViewModel = new NetAddressTestViewModel((NetAddress)_dcServiceAddresses.Main.Clone(), _clientIni, _logFile, _clientId);
            ServerConnectionTestViewModel.PropertyChanged += ServerConnectionTestViewModel_PropertyChanged;
        }

        private void ServerConnectionTestViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Result")
            {
                Message = ServerConnectionTestViewModel.Result ? "Connection established successfully!" : "Can't establish connection!";
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
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}