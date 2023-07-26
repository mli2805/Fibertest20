using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.SuperClient
{
    public class ServersViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly IMyLog _logFile;
        private readonly GasketViewModel _gasketViewModel;
        private readonly ChildStarter _childStarter;
        private readonly AddServerViewModel _addServerViewModel;
        private readonly D2CWcfManager _d2CWcfManager;

        public FtServerList FtServerList { get; set; }
        private FtServer _selectedFtServer;
        public FtServer SelectedFtServer
        {
            get { return _selectedFtServer; }
            set
            {
                if (Equals(value, _selectedFtServer)) return;
                _selectedFtServer = value;
                NotifyOfPropertyChange();
                if (_selectedFtServer != null)
                    _gasketViewModel.BringTabItemToFront(_selectedFtServer.Entity.Postfix);
            }
        }

        public ServersViewModel(IWindowManager windowManager, IMyLog logFile,
            FtServerList ftServerList, GasketViewModel gasketViewModel,
            ChildStarter childStarter, AddServerViewModel addServerViewModel,
            D2CWcfManager d2CWcfManager)
        {
            _windowManager = windowManager;
            _logFile = logFile;
            FtServerList = ftServerList;
            FtServerList.DeserializeAndDecrypt();
            _gasketViewModel = gasketViewModel;
            SelectedFtServer = FtServerList.Servers.FirstOrDefault();
            _childStarter = childStarter;
            _addServerViewModel = addServerViewModel;
            _d2CWcfManager = d2CWcfManager;
        }

        public void ConnectServer()
        {
            _logFile.AppendLine($@"User asks connection to {SelectedFtServer.Entity.ServerTitle}");
            _childStarter.StartFtClient(SelectedFtServer.Entity);
        }

        public void SetConnectionResult(int postfix, bool isLoadedOk, bool isStateOk)
        {
            var server = FtServerList.Servers.FirstOrDefault(s => s.Entity.Postfix == postfix);
            if (server == null) return;

            if (!isLoadedOk)
            {
                server.ServerConnectionState = FtServerConnectionState.Breakdown;
                var strs = new List<string>()
                {
                    Resources.SID_Failed_to_establish_connection_,
                    $@"{server.ServerName}",
                };
                var vm = new MyMessageBoxViewModel(MessageType.Error, strs, 2);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            server.ServerConnectionState = FtServerConnectionState.Connected;
            server.SystemState = isStateOk ? FtSystemState.AllIsOk : FtSystemState.ThereAreAnyProblem;
        }

        public void CleanBrokenConnection(int postfix)
        {
            var server = FtServerList.Servers.FirstOrDefault(s => s.Entity.Postfix == postfix);
            if (server == null) return;

            server.ServerConnectionState = FtServerConnectionState.Breakdown;
            server.SystemState = FtSystemState.Unknown;
            _childStarter.CleanAfterClosing(server.Entity);
        }

        private void ChangeSelectedClient()
        {
            var selectedFtServer =
                FtServerList.Servers.FirstOrDefault(s => s.ServerConnectionState == FtServerConnectionState.Connected);
            if (selectedFtServer != null)
                SelectedFtServer = selectedFtServer;
        }

        public void ChangeSelectedClient(int postfix)
        {
            var selectedFtServer = FtServerList.Servers.FirstOrDefault(s => s.Entity.Postfix == postfix);
            if (selectedFtServer != null)
                SelectedFtServer = selectedFtServer;
        }

        private async Task CloseClient(FtServer ftServer)
        {
            var ftClientAddress = new NetAddress() { Ip4Address = @"localhost", Port = 11843 + ftServer.Entity.Postfix };
            _d2CWcfManager.SetClientsAddresses(new List<DoubleAddress>() { new DoubleAddress() { Main = ftClientAddress } });
            await _d2CWcfManager.SuperClientAsksClientToExit();
            _childStarter.CleanAfterClosing(ftServer.Entity);
            ftServer.ServerConnectionState = FtServerConnectionState.Disconnected;
            ftServer.SystemState = FtSystemState.Unknown;
        }

        public async Task CloseAllClients()
        {
            foreach (var ftServer in FtServerList.Servers.Where(s => s.ServerConnectionState == FtServerConnectionState.Connected).ToList())
            {
                await CloseClient(ftServer);
            }
        }

        public void SetServerIsClosed(int postfix)
        {
            var server = FtServerList.Servers.FirstOrDefault(s => s.Entity.Postfix == postfix);
            if (server != null)
                server.ServerConnectionState = FtServerConnectionState.Disconnected;
        }

        public void SetSystemState(int postfix, bool isStateOk)
        {
            var server = FtServerList.Servers.FirstOrDefault(s => s.Entity.Postfix == postfix);
            if (server != null)
                server.SystemState = isStateOk ? FtSystemState.AllIsOk : FtSystemState.ThereAreAnyProblem;
        }

        public void AddServer()
        {
            _addServerViewModel.Init(null);
            _windowManager.ShowDialog(_addServerViewModel);
        }

        public void EditServer()
        {
            _addServerViewModel.Init(_selectedFtServer.Entity);
            _windowManager.ShowDialog(_addServerViewModel);
            _selectedFtServer.NotifyOfPropertyChange(nameof(_selectedFtServer.ServerName));
        }

        public async void CloseSelectedClient()
        {
            await CloseClient(SelectedFtServer);
            ChangeSelectedClient();
        }

        public async void RemoveServer()
        {
            await CloseClient(SelectedFtServer);
            FtServerList.Remove(SelectedFtServer);
            ChangeSelectedClient();
        }


    }
}
