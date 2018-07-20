using System.Linq;
using Caliburn.Micro;

namespace Iit.Fibertest.SuperClient
{
    public class ServersViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly GasketViewModel _gasketViewModel;
        private readonly ChildStarter _childStarter;
        private readonly AddServerViewModel _addServerViewModel;

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
            ChildStarter childStarter, AddServerViewModel addServerViewModel)
        {
            _windowManager = windowManager;
            FtServerList = ftServerList;
            FtServerList.Read();
            _gasketViewModel = gasketViewModel;
            _childStarter = childStarter;
            _addServerViewModel = addServerViewModel;
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
