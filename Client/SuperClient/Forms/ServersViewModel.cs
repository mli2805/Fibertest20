using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.SuperClient
{
    public class ServersViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
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
                _gasketViewModel.BringTabItemToFront(_selectedFtServer.Entity.Postfix);
            }
        }

        public ServersViewModel(IWindowManager windowManager, 
            FtServerList ftServerList, GasketViewModel gasketViewModel,
            ChildStarter childStarter, AddServerViewModel addServerViewModel,
            D2CWcfManager d2CWcfManager)
        {
            _windowManager = windowManager;
            FtServerList = ftServerList;
            FtServerList.Read();
            _gasketViewModel = gasketViewModel;
            _childStarter = childStarter;
            _addServerViewModel = addServerViewModel;
            _d2CWcfManager = d2CWcfManager;
        }

        public void ConnectServer()
        {
            _childStarter.StartFtClient(SelectedFtServer.Entity);
        }

        public void SetServerIsReady(int postfix)
        {
            var server = FtServerList.Servers.FirstOrDefault(s => s.Entity.Postfix == postfix);
            if (server != null)
                server.ServerConnectionState = FtServerState.Connected;
        }

        public void DisconnectServer()
        {
            _childStarter.CloseFtClient(SelectedFtServer.Entity);
        }

        public async void CloseClient()
        {
            var ftClientAddress = new NetAddress(){Ip4Address = "localhost", Port = 11843 + SelectedFtServer.Entity.Postfix};
            _d2CWcfManager.SetClientsAddresses(new List<DoubleAddress>(){new DoubleAddress(){Main = ftClientAddress}});
            await _d2CWcfManager.AskClientToExit();
        }

        public void SetServerIsClosed(int postfix)
        {
            var server = FtServerList.Servers.FirstOrDefault(s => s.Entity.Postfix == postfix);
            if (server != null)
                server.ServerConnectionState = FtServerState.Disconnected;
        }

        public void AddServer()
        {
            _windowManager.ShowDialog(_addServerViewModel);
        }

        public void RemoveServer()
        {
            DisconnectServer();
            FtServerList.Remove(SelectedFtServer);
        }

    
    }
}
