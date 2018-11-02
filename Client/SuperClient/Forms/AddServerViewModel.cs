using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.SuperClient
{
    public class AddServerViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly FtServerList _ftServerList;
        private readonly C2DWcfManager _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public string ServerTitle { get; set; }

        public string ServerIp { get; set; }

        public int ServerTcpPort { get; set; } = 11840;

        public string Username { get; set; } = @"superclient";

        public string Password { get; set; } = @"superclient";

        public AddServerViewModel(ILifetimeScope globalScope, FtServerList ftServerList,
            C2DWcfManager c2DWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _ftServerList = ftServerList;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Add_server;
            ServerTitle = "";
            ServerIp = "";
        }

        public async void CheckConnection()
        {
            var addressForTesting = new DoubleAddress()
            {
                HasReserveAddress = false,
                Main = new NetAddress(ServerIp, ServerTcpPort),
            };
            bool result;

            using (_globalScope.Resolve<IWaitCursor>())
            {
                _c2DWcfManager.SetServerAddresses(addressForTesting, "", "");
                result = await _c2DWcfManager.CheckServerConnection(new CheckServerConnectionDto());
            }

            var message = result
                 ? Resources.SID_Connection_established_successfully_
                 : Resources.SID_Cant_establish_connection_;
            var messageBoxModel = new MyMessageBoxViewModel(MessageType.Information, message);
            _windowManager.ShowDialogWithAssignedOwner(messageBoxModel);
        }

        public void Save()
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
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }

    }
}
