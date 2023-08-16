using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.SuperClient
{
    public class AddServerViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly FtServerList _ftServerList;
        private readonly DesktopC2DWcfManager _desktopC2DWcfManager;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly IWindowManager _windowManager;

        private bool _isAddMode;
        private FtServerEntity _entity;

        public string ServerTitle { get; set; }

        public string ServerIp { get; set; }

        public int ServerTcpPort { get; set; }

        private string _serverVersion;
        public string ServerVersion
        {
            get => _serverVersion;
            set
            {
                if (value == _serverVersion) return;
                _serverVersion = value;
                NotifyOfPropertyChange();
            }
        }

        private string _clientVersion;
        public string ClientVersion
        {
            get => _clientVersion;
            set
            {
                if (value == _clientVersion) return;
                _clientVersion = value;
                NotifyOfPropertyChange();
            }
        }

        private string _clientFolder;
        public string ClientFolder
        {
            get => _clientFolder;
            set
            {
                if (value == _clientFolder) return;
                _clientFolder = value;
                NotifyOfPropertyChange();
            }
        }

        public string Username { get; set; } = @"superclient";

        public string Password { get; set; } = @"superclient";

        public AddServerViewModel(ILifetimeScope globalScope, FtServerList ftServerList,
            DesktopC2DWcfManager desktopC2DWcfManager, CommonC2DWcfManager commonC2DWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _ftServerList = ftServerList;
            _desktopC2DWcfManager = desktopC2DWcfManager;
            _commonC2DWcfManager = commonC2DWcfManager;
            _windowManager = windowManager;
        }

        public void Init(FtServerEntity serverEntity)
        {
            _entity = serverEntity;
            if (serverEntity == null)
            {
                _isAddMode = true;
                ServerTitle = "";
                ServerIp = "";
                ServerTcpPort = (int)TcpPorts.ServerListenToDesktopClient;
                Username = @"superclient";
                Password = @"superclient";

                ServerVersion = "";
                ClientVersion = "";
                ClientFolder = "";
            }
            else
            {
                _isAddMode = false;
                ServerTitle = serverEntity.ServerTitle;
                ServerIp = serverEntity.ServerIp;
                ServerTcpPort = serverEntity.ServerTcpPort;
                ServerVersion = serverEntity.ServerVersion;
                ClientVersion = serverEntity.ClientVersion;
                ClientFolder = serverEntity.ClientFolder;
                Username = serverEntity.Username;
                Password = serverEntity.Password;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isAddMode ? Resources.SID_Add_server : Resources.SID_Edit_settings;
        }

        public async void CheckAddress()
        {
            var addressForTesting = new DoubleAddress()
            {
                HasReserveAddress = false,
                Main = new NetAddress(ServerIp, ServerTcpPort),
            };
            bool result;

            using (_globalScope.Resolve<IWaitCursor>())
            {
                _desktopC2DWcfManager.SetServerAddresses(addressForTesting, "", "");
                result = await _desktopC2DWcfManager.CheckServerConnection(new CheckServerConnectionDto());
            }

            var message = result
                 ? Resources.SID_Connection_established_successfully_
                 : Resources.SID_Cant_establish_connection_;
            var messageBoxModel = new MyMessageBoxViewModel(MessageType.Information, message);
            _windowManager.ShowDialogWithAssignedOwner(messageBoxModel);
        }

        public async void CheckConnectionButton()
        {
            var result = await CheckConnection();
            ProcessResult(result);
        }

        private async Task<ClientRegisteredDto> CheckConnection()
        {
            var desktopServiceAddress = new DoubleAddress()
            {
                HasReserveAddress = false,
                Main = new NetAddress(ServerIp, ServerTcpPort),
            };
            var commonServiceAddress = (DoubleAddress)desktopServiceAddress.Clone();
            commonServiceAddress.Main.Port = (int)TcpPorts.ServerListenToCommonClient;
            _commonC2DWcfManager.SetServerAddresses(commonServiceAddress, Username, "");

            var connectionId = Guid.NewGuid().ToString();
            var clientAddress = new DoubleAddress()
            {   // client's address does not matter here
                Main = new NetAddress(@"1.1.1.1", TcpPorts.ClientListenTo)
            };
            var dto = new RegisterClientDto()
            {
                UserName = Username,
                Password = Password.GetHashString(),
                ConnectionId = connectionId,
                Addresses = clientAddress,
                IsUnderSuperClient = true,
            };
            ClientRegisteredDto result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                result = await _commonC2DWcfManager.RegisterClientAsync(dto);
                if (result.ReturnCode == ReturnCode.ClientRegisteredSuccessfully)
                {
                    var unDto = new UnRegisterClientDto()
                    {
                        ConnectionId = connectionId,
                        Username = Username,
                        ClientIp = clientAddress.Main.Ip4Address,
                    };
                    await _commonC2DWcfManager.UnregisterClientAsync(unDto);
                }
            }
            return result;
        }


        private void ProcessResult(ClientRegisteredDto result)
        {
            if (result.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, result.ReturnCode.GetLocalizedString());
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            ServerVersion = result.DatacenterVersion;

            var clientList = Utils.GetClients();
            if (!clientList.IsSuccess)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, clientList.ErrorMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            var list = new List<string>
            {
                Resources.SID_Connection_established_successfully_,
                Resources.SID_Remote_Server_software_version_is_+ result.DatacenterVersion,
                "",
                Resources.SID_Matching_version_of_Client_software
            };

            var match = clientList.Clients.FirstOrDefault(c => c.Version == result.DatacenterVersion);

            if (match != null)
            {
                list.Add(Resources.SID_found_);
                list.Add($@"({match.Path})");
                list.Add("");

                ClientVersion = match.Version;
                ClientFolder = match.Path;

            }
            else
            {
                list.Add(Resources.SID_not_found_);
                list.Add("");
            }

            _windowManager.ShowDialogWithAssignedOwner(
                new MyMessageBoxViewModel(MessageType.Information, list, 4));
        }

        public void SelectClient()
        {
            var vm = new ClientSelectionViewModel();
            var clientList = Utils.GetClients();
            if (clientList == null)
            {
                clientList = Utils.GetClients();
                if (!clientList.IsSuccess)
                {
                    var mb = new MyMessageBoxViewModel(MessageType.Error, clientList.ErrorMessage);
                    _windowManager.ShowDialogWithAssignedOwner(mb);
                    return;
                }
            }

            vm.Initialize(ServerVersion, clientList);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            if (vm.IsApplyPressed)
            {
                var cl = vm.Clients.First(c => c.IsChecked);
                ClientVersion = cl.Version;
                ClientFolder = cl.Path;
            }
        }

        public void Save()
        {
            if (_isAddMode)
                SaveNew();
            else
                UpdateExisted();
            TryClose();
        }

        private void SaveNew()
        {
            var maxPostfix = _ftServerList.Servers.Any() ? _ftServerList.Servers.Select(x => x.Entity).Max(e => e.Postfix) : 0;

            var ftServerEntity = new FtServerEntity()
            {
                Postfix = maxPostfix + 1,
                ServerTitle = ServerTitle,
                ServerIp = ServerIp,
                ServerTcpPort = ServerTcpPort,
                ServerVersion = ServerVersion,
                ClientVersion = ClientVersion,
                ClientFolder = ClientFolder,
                Username = Username,
                Password = Password,
            };
            _ftServerList.Add(new FtServer() { Entity = ftServerEntity });
        }

        private void UpdateExisted()
        {
            _entity.ServerTitle = ServerTitle;
            _entity.ServerIp = ServerIp;
            _entity.ServerTcpPort = ServerTcpPort;
            _entity.ServerVersion = ServerVersion;
            _entity.ClientVersion = ClientVersion;
            _entity.ClientFolder = ClientFolder;
            _entity.Username = Username;
            _entity.Password = Password;

            _ftServerList.EncryptAndSerialize();
        }

        public void Cancel()
        {
            TryClose();
        }

    }
}
