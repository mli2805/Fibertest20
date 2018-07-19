using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using System.Diagnostics;
using System.Windows.Controls;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.SuperClient
{
    public class ShellViewModel : Screen, IShell
    {
        private Dictionary<int, int> _postfixToTabitem = new Dictionary<int, int>();
        private Dictionary<int, Process> _processes = new Dictionary<int, Process>();
        public ObservableCollection<TabItem> Children { get; set; } = new ObservableCollection<TabItem>();

        private int _selectedTabItemIndex;
        public int SelectedTabItemIndex
        {
            get => _selectedTabItemIndex;

            set
            {
                if (_selectedTabItemIndex == value) return;
                _selectedTabItemIndex = value;
                NotifyOfPropertyChange();
            }
        }



        private readonly IMyLog _logFile;
        private IWindowManager _windowManager;
        private ChildStarter _childStarter;
        private AddServerViewModel _addServerViewModel;
        private FtServer _selectedFtServer;
        public FtServerList FtServerList { get; set; }

        public FtServer SelectedFtServer
        {
            get { return _selectedFtServer; }
            set
            {
                if (Equals(value, _selectedFtServer)) return;
                _selectedFtServer = value;
                NotifyOfPropertyChange();
                if (_postfixToTabitem.ContainsKey(_selectedFtServer.Entity.Postfix))
                    SelectedTabItemIndex = _postfixToTabitem[_selectedFtServer.Entity.Postfix];
            }
        }

        public ShellViewModel(IMyLog logFile, IWindowManager windowManager, FtServerList ftServerList,
            ChildStarter childStarter, AddServerViewModel addServerViewModel)
        {
            _logFile = logFile;
            _windowManager = windowManager;
            FtServerList = ftServerList;
            FtServerList.Read();
            _childStarter = childStarter;
            _addServerViewModel = addServerViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0 Superclient";
            _logFile.AssignFile(@"sc.log");
            _logFile.AppendLine(@"Super-Client application started!");
        }

        public void ConnectServer()
        {
            var tabItem = new TabItem() { Header = new ContentControl() };
            Children.Add(tabItem);
            SelectedTabItemIndex = Children.Count -1;

            var panel = _childStarter.CreatePanel(tabItem);

            var process = _childStarter.StartChild(SelectedFtServer.Entity);
            _processes.Add(SelectedFtServer.Entity.Postfix, process);

            _childStarter.PutChildOnPanel(process, panel);
            SelectedFtServer.ServerConnectionState = FtServerState.Connected;
            _postfixToTabitem.Add(SelectedFtServer.Entity.Postfix, SelectedTabItemIndex);
        }

        public void DisconnectServer()
        {
            var process = _processes[SelectedFtServer.Entity.Postfix];
            process.Kill();
            _processes.Remove(SelectedFtServer.Entity.Postfix);

            var tabIndex = _postfixToTabitem[SelectedFtServer.Entity.Postfix];
            Children.RemoveAt(tabIndex);
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