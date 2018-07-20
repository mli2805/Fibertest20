using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.SuperClient
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IMyLog _logFile;
        private IWindowManager _windowManager;
        private readonly SuperClientWcfServiceHost _superClientWcfServiceHost;
        private ChildStarter _childStarter;
        private AddServerViewModel _addServerViewModel;
        public GasketViewModel GasketViewModel { get; set; }

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
                GasketViewModel.BringTabItemToFront(_selectedFtServer.Entity.Postfix);
            }
        }

        public ShellViewModel(IMyLog logFile, IWindowManager windowManager,
            SuperClientWcfServiceHost superClientWcfServiceHost, FtServerList ftServerList,
            GasketViewModel gasketViewModel,
            ChildStarter childStarter, AddServerViewModel addServerViewModel)
        {
            _logFile = logFile;
            _windowManager = windowManager;
            _superClientWcfServiceHost = superClientWcfServiceHost;
            FtServerList = ftServerList;
            FtServerList.Read();
            GasketViewModel = gasketViewModel;
            _childStarter = childStarter;
            _addServerViewModel = addServerViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0 Superclient";
            _logFile.AssignFile(@"sc.log");
            _logFile.AppendLine(@"Super-Client application started!");
            _superClientWcfServiceHost.StartWcfListener();
        }

        public void ConnectServer()
        {
            _childStarter.StartFtClient(SelectedFtServer.Entity);
            SelectedFtServer.ServerConnectionState = FtServerState.Connected;
        }

        public void DisconnectServer()
        {
            _childStarter.CloseFtClient(SelectedFtServer.Entity);
            SelectedFtServer.ServerConnectionState = FtServerState.Disconnected;
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