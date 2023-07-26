using System;
using System.Linq;
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
        public string ServerVersion { get; set; }

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
            }
            else
            {
                _isAddMode = false;
                ServerTitle = serverEntity.ServerTitle;
                ServerIp = serverEntity.ServerIp;
                ServerTcpPort = serverEntity.ServerTcpPort;
                ServerVersion = serverEntity.ServerVersion;
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

        public async void CheckConnection()
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
                    ServerVersion = result.DatacenterVersion;
                    NotifyOfPropertyChange(nameof(ServerVersion));
                    var unDto = new UnRegisterClientDto()
                    {
                        ConnectionId = connectionId,
                        Username = Username,
                        ClientIp = clientAddress.Main.Ip4Address,
                    };
                    await _commonC2DWcfManager.UnregisterClientAsync(unDto);
                }
            }

            var message = result.ReturnCode == ReturnCode.ClientRegisteredSuccessfully
                ? Resources.SID_Connection_established_successfully_
                : result.ReturnCode.GetLocalizedString();

            var vm = new MyMessageBoxViewModel(MessageType.Information, message);
            _windowManager.ShowDialogWithAssignedOwner(vm);
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
