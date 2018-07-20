using System.Collections.Generic;
using Caliburn.Micro;
using System.Diagnostics;
using System.Windows.Controls;
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

        private Dictionary<int, int> _postfixToTabitem = new Dictionary<int, int>();
        private Dictionary<int, Process> _processes = new Dictionary<int, Process>();

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
                if (_postfixToTabitem.ContainsKey(_selectedFtServer.Entity.Postfix))
                    GasketViewModel.SelectedTabItemIndex = _postfixToTabitem[_selectedFtServer.Entity.Postfix];
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
            var tabItem = new TabItem() { Header = new ContentControl() };
            GasketViewModel.Children.Add(tabItem);
            GasketViewModel.SelectedTabItemIndex = GasketViewModel.Children.Count -1;

            var panel = _childStarter.CreatePanel(tabItem);

            var process = _childStarter.StartChild(SelectedFtServer.Entity);
            _processes.Add(SelectedFtServer.Entity.Postfix, process);

            _childStarter.PutChildOnPanel(process, panel);
            SelectedFtServer.ServerConnectionState = FtServerState.Connected;
            _postfixToTabitem.Add(SelectedFtServer.Entity.Postfix, GasketViewModel.SelectedTabItemIndex);
        }

        public void DisconnectServer()
        {
            var process = _processes[SelectedFtServer.Entity.Postfix];
            process.Kill();
            _processes.Remove(SelectedFtServer.Entity.Postfix);

            var tabIndex = _postfixToTabitem[SelectedFtServer.Entity.Postfix];
            GasketViewModel.Children.RemoveAt(tabIndex);
            _postfixToTabitem.Remove(SelectedFtServer.Entity.Postfix);

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