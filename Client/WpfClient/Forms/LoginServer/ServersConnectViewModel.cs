using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class ServersConnectViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        public string NewServerTitle { get; set; }

        public Visibility NewServerTitleVisibility
        {
            get => _newServerTitleVisibility;
            set
            {
                if (value == _newServerTitleVisibility) return;
                _newServerTitleVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public List<Server> Servers { get; set; }

        public Visibility ServersComboboxVisibility
        {
            get => _serversComboboxVisibility;
            set
            {
                if (value == _serversComboboxVisibility) return;
                _serversComboboxVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Server _selectedServer;
        public Server SelectedServer
        {
            get => _selectedServer;
            set
            {
                if (Equals(value, _selectedServer)) return;
                _selectedServer = value;
                NotifyOfPropertyChange();
                ToggleToSelectServerMode();
            }
        }

        private string _clientAddress;

        public NetAddressTestViewModel ServerConnectionTestViewModel
        {
            get => _serverConnectionTestViewModel;
            set
            {
                if (Equals(value, _serverConnectionTestViewModel)) return;
                _serverConnectionTestViewModel = value;
                NotifyOfPropertyChange();
            }
        }

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

        public ServersConnectViewModel(ILifetimeScope globalScope, IniFile iniFile, IMyLog logFile)
        {
            _globalScope = globalScope;
            _iniFile = iniFile;
            _logFile = logFile;
            InitializeView();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Server;
            Message = Resources.SID_Enter_DataCenter_Server_address;
        }

        private void InitializeView()
        {
            Servers = ServerList.Load(_logFile);
            SelectedServer = Servers.FirstOrDefault(s=>s.IsLastSelected) ?? Servers.FirstOrDefault();
            if (SelectedServer == null)
                ToggleToAddServerMode();
            else
                ToggleToSelectServerMode();
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

        private bool _isInAddMode;
        private NetAddress _serverInWorkAddress = new NetAddress(@"0.0.0.0", TcpPorts.ServerListenToClient);
        private Visibility _serversComboboxVisibility = Visibility.Visible;
        private Visibility _newServerTitleVisibility = Visibility.Collapsed;
        private NetAddressTestViewModel _serverConnectionTestViewModel;

        public void ButtonPlus()
        {
            if (!_isInAddMode)
             ToggleToAddServerMode();
        }

        private void ToggleToAddServerMode()
        {
            ServersComboboxVisibility = Visibility.Collapsed;

            ServerConnectionTestViewModel = _globalScope.Resolve<NetAddressTestViewModel>
                (new NamedParameter(@"netAddressForConnectionTest", new NetAddressForConnectionTest((NetAddress)_serverInWorkAddress.Clone(), false)));
            ServerConnectionTestViewModel.PropertyChanged += ServerConnectionTestViewModel_PropertyChanged;

            NewServerTitleVisibility = Visibility.Visible;
            _isInAddMode = true;
        }

        private void ToggleToSelectServerMode()
        {
            ServersComboboxVisibility = Visibility.Visible;

            ServerConnectionTestViewModel = _globalScope.Resolve<NetAddressTestViewModel>
                (new NamedParameter(@"netAddressForConnectionTest", new NetAddressForConnectionTest((NetAddress)(NetAddress)SelectedServer.ServerAddress.Main.Clone(), false)));
            ServerConnectionTestViewModel.PropertyChanged += ServerConnectionTestViewModel_PropertyChanged;

            NewServerTitleVisibility = Visibility.Collapsed;
            _isInAddMode = false;
        }

        public void Cancel()
        {
            if (!_isInAddMode || !Servers.Any()) TryClose();

            ToggleToSelectServerMode();
        }

        private void AddServerIntoList()
        {
            var newServer = new Server()
            {
                Title = NewServerTitle,
                ServerAddress = new DoubleAddress() { Main = _serverInWorkAddress },
                ClientIpAddress = _clientAddress,
                IsLastSelected = true,
            };

            Servers.Add(newServer);
            SelectedServer = newServer;
        }

        public void Save()
        {
            if (_isInAddMode)
                AddServerIntoList();
            else
                Servers.ForEach(s => s.IsLastSelected = s.Equals(SelectedServer));

            SelectedServer.ServerAddress.Main = (NetAddress)ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress().Clone();
            _iniFile.WriteServerAddresses(SelectedServer.ServerAddress);

            var clientAddress = new NetAddress(_clientAddress, TcpPorts.ClientListenTo);
            _iniFile.Write(clientAddress, IniSection.ClientLocalAddress);

            ServerList.Save(Servers, _logFile);

            TryClose(true);
        }
    }
}
