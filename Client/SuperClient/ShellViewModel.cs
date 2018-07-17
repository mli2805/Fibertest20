using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using System.Diagnostics;
using System.Windows.Controls;
using Panel = System.Windows.Forms.Panel;
using System;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.SuperClient
{
    public class ShellViewModel : Screen, IShell
    {
        public ObservableCollection<TabItem> TabItems { get; set; } = new ObservableCollection<TabItem>();
        private int _selectedItemIndex;

        public int SelectedItemIndex
        {
            get => _selectedItemIndex;
          
            set   
            {
                if (_selectedItemIndex == value) return;
                _selectedItemIndex = value;
                NotifyOfPropertyChange();
            } 
        }
        public List<Panel> Panels = new List<Panel>();
        public List<Tuple<string, string>> ServersTuples;
        public int LastUsed = -1;
        public List<Process> Processes = new List<Process>();


        private readonly IMyLog _logFile;
        private IWindowManager _windowManager;
        private readonly FtServerList _ftServerList;
        private ChildStarter _childStarter;
        private AddServerViewModel _addServerViewModel;

        public ObservableCollection<FtServer> Servers { get; set; }

        public ShellViewModel(IMyLog logFile, IWindowManager windowManager, FtServerList ftServerList, 
            ChildStarter childStarter, AddServerViewModel addServerViewModel)
        {
            ServersTuples = new List<Tuple<string, string>>();
            ServersTuples.Add(new Tuple<string, string>("192.168.96.21", "11840"));
            ServersTuples.Add(new Tuple<string, string>("172.16.4.105", "11840"));
            ServersTuples.Add(new Tuple<string, string>("172.16.4.115", "11840"));
            ServersTuples.Add(new Tuple<string, string>("172.16.4.100", "11840"));

            _logFile = logFile;
            _windowManager = windowManager;
            _ftServerList = ftServerList;
            _childStarter = childStarter;
            _addServerViewModel = addServerViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0 Superclient";
            _logFile.AssignFile(@"sc.log");
            _logFile.AppendLine(@"Super-Client application started!");
            
            Servers = _ftServerList.Read();

        }

        public void AddChild()
        {
            var tabItem = new TabItem() { Header = new ContentControl() };
            TabItems.Add(tabItem);
            var panel = _childStarter.CreatePanel(tabItem);
            Panels.Add(panel);
            LastUsed++;
            SelectedItemIndex = LastUsed;
            var process = _childStarter.StartChild(LastUsed, ServersTuples[LastUsed].Item1, ServersTuples[LastUsed].Item2);
            Processes.Add(process);
            _childStarter.PutChildOnPanel(process, panel);
        }

        public void AddServer()
        {
            _windowManager.ShowDialog(_addServerViewModel);
        }
    }
}