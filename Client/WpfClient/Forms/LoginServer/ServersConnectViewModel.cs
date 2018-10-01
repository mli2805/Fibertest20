using System.Collections.ObjectModel;
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

        public ObservableCollection<Server> Servers { get; set; }

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

        public bool IsRemoveServerEnabled
        {
            get { return _isRemoveServerEnabled; }
            set
            {
                if (value == _isRemoveServerEnabled) return;
                _isRemoveServerEnabled = value;
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
                if (_selectedServer != null)
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
            Servers = new ObservableCollection<Server>();
            ServerList.Load(_iniFile, _logFile).ForEach(s => Servers.Add(s));
            SelectedServer = Servers.FirstOrDefault(s => s.IsLastSelected) ?? Servers.FirstOrDefault();
            IsRemoveServerEnabled = Servers.Count > 0;
            if (SelectedServer == null)
                ToggleToAddServerMode();
            else
                ToggleToSelectServerMode();
        }

        private void ServerConnectionTestViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Result")
            {
                if (ServerConnectionTestViewModel.Result == true)
                {
                    var netAddress = ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress();
                    var serverAddress = netAddress.IsAddressSetAsIp ? netAddress.Ip4Address : netAddress.HostName;
                    if (serverAddress == @"localhost")
                    {
                        var serverIp = LocalAddressResearcher.GetAllLocalAddresses().First();
                        ServerConnectionTestViewModel.NetAddressInputViewModel =
                            new NetAddressInputViewModel(new NetAddress(serverIp, TcpPorts.ServerListenToClient), true);
                        serverAddress = serverIp;
                    }
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
        private readonly NetAddress _serverInWorkAddress = new NetAddress(@"0.0.0.0", TcpPorts.ServerListenToClient);
        private Visibility _serversComboboxVisibility = Visibility.Visible;
        private Visibility _newServerTitleVisibility = Visibility.Collapsed;
        private NetAddressTestViewModel _serverConnectionTestViewModel;
        private bool _isRemoveServerEnabled;

        public void ButtonPlus()
        {
            if (!_isInAddMode)
                ToggleToAddServerMode();
        }

        public void ButtonMinus()
        {
            Servers.Remove(SelectedServer);
            SaveChanges();

            if (Servers.Count > 0)
                SelectedServer = Servers.First(s=>!s.Equals(SelectedServer));
            else
                ToggleToAddServerMode();

            IsRemoveServerEnabled = Servers.Count > 0;
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

            var address = (NetAddress)SelectedServer?.ServerAddress.Main.Clone() ?? new NetAddress(@"0.0.0.0", 11840);
            ServerConnectionTestViewModel = _globalScope.Resolve<NetAddressTestViewModel>
                (new NamedParameter(@"netAddressForConnectionTest", new NetAddressForConnectionTest(address, false)));
            ServerConnectionTestViewModel.PropertyChanged += ServerConnectionTestViewModel_PropertyChanged;

            NewServerTitleVisibility = Visibility.Collapsed;
            _isInAddMode = false;
        }

        public void Cancel()
        {
            if (!_isInAddMode) TryClose();

            ToggleToSelectServerMode();
        }

        private void AddServerIntoList()
        {
            var newServer = new Server()
            {
                Title = NewServerTitle,
                ServerAddress = new DoubleAddress() { Main = (NetAddress)ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress().Clone() },
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

            SaveChanges();
            TryClose(true);
        }

        private void SaveChanges()
        {
            var serversList = Servers.ToList();
            serversList.ForEach(s => s.IsLastSelected = s.Equals(SelectedServer));

            if (SelectedServer != null)
            {
                SelectedServer.ServerAddress.Main = (NetAddress)ServerConnectionTestViewModel.NetAddressInputViewModel.GetNetAddress().Clone();
                _iniFile.WriteServerAddresses(SelectedServer.ServerAddress);
                _iniFile.Write(IniSection.Server, IniKey.ServerTitle, SelectedServer.Title);
            }

            var clientAddress = new NetAddress(_clientAddress, TcpPorts.ClientListenTo);
            _iniFile.Write(clientAddress, IniSection.ClientLocalAddress);

            ServerList.Save(serversList, _iniFile, _logFile);
        }
    }
}
