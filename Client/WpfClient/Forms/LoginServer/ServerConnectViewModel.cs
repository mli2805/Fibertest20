using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class ServerConnectViewModel : Screen
    {
        private readonly IniFile _clientIni;
        private DoubleAddress _dcServiceAddresses;
        private string _clientAddress;
        public NetAddressTestViewModel ServerConnectionTestViewModel { get; set; }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                NotifyOfPropertyChange();
            }
        }

        public ServerConnectViewModel(ILifetimeScope globalScope, IniFile iniFile)
        {
            _clientIni = iniFile;
            _dcServiceAddresses = _clientIni.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);

            var localScope = globalScope.BeginLifetimeScope(
                ctx => ctx.RegisterInstance(
                    new NetAddressForConnectionTest((NetAddress)_dcServiceAddresses.Main.Clone(), false)));
            ServerConnectionTestViewModel = localScope.Resolve<NetAddressTestViewModel>();
            ServerConnectionTestViewModel.PropertyChanged += ServerConnectionTestViewModel_PropertyChanged;
        }

        private void ServerConnectionTestViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Result")
            {
                if (ServerConnectionTestViewModel.Result == true)
                {
                    var serverAddress = ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address;
                    _clientAddress = LocalAddressResearcher.GetLocalAddressToConnectServer(serverAddress);
                    Message = Resources.SID_Connection_established_successfully_
                        + System.Environment.NewLine + string.Format(Resources.SID___Client_s_address__0_, _clientAddress);
                }
                else
                    Message = ServerConnectionTestViewModel.Result == null
                        ? Resources.SID_Establishing_connection___
                        : Resources.SID_Cant_establish_connection_;
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

            var clientAddress = new NetAddress(_clientAddress, TcpPorts.ClientListenTo);
            _clientIni.Write(clientAddress, IniSection.ClientLocalAddress);

            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}