using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.Win32;

namespace KadastrLoader
{
    public class KadastrLoaderViewModel : Screen, IShell
    {
        private readonly LoadedAlready _loadedAlready;
        private readonly KadastrDbProvider _kadastrDbProvider;
        private readonly KadastrFilesParser _kadastrFilesParser;
        private readonly DesktopC2DWcfManager _desktopC2DWcfManager;
        public string ServerIp { get; set; }
        public int MySqlPort { get; set; }

        private string _kadastrMessage;
        public string KadastrMessage
        {
            get { return _kadastrMessage; }
            set
            {
                if (value == _kadastrMessage) return;
                _kadastrMessage = value;
                NotifyOfPropertyChange();
            }
        }

        private string _serverMessage;
        public string ServerMessage
        {
            get => _serverMessage;
            set
            {
                if (value == _serverMessage) return;
                _serverMessage = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsStartEnabled => _isDbReady && _isDataCenterReady && _isFolderValid;

        private string _selectedFolder;
        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (value == _selectedFolder) return;
                _selectedFolder = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsFree
        {
            get { return _isFree; }
            set
            {
                if (value == _isFree) return;
                _isFree = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<string> ProgressLines { get; set; } = new ObservableCollection<string>();

        public KadastrLoaderViewModel(IniFile iniFile, LoadedAlready loadedAlready,
            KadastrDbProvider kadastrDbProvider,
            KadastrFilesParser kadastrFilesParser, DesktopC2DWcfManager desktopC2DWcfManager)
        {
            _loadedAlready = loadedAlready;
            _kadastrDbProvider = kadastrDbProvider;
            _kadastrFilesParser = kadastrFilesParser;
            _desktopC2DWcfManager = desktopC2DWcfManager;
            var serverAddresses = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToDesktopClient);
            desktopC2DWcfManager.SetServerAddresses(serverAddresses, "Kadastr", "");
            ServerIp = serverAddresses.Main.Ip4Address;
            MySqlPort = iniFile.Read(IniSection.MySql, IniKey.MySqlTcpPort, 33060);
            IsFree = true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Load_from_Kadastr;
        }

        private bool _isDbReady;
        private bool _isDataCenterReady;

        public async void CheckConnect()
        {
            _isDataCenterReady = await CheckDataCenter();
            _isDbReady = await ConnectKadastrDb();
            NotifyOfPropertyChange(nameof(IsStartEnabled));
        }
        private async Task<bool> ConnectKadastrDb()
        {
            try
            {
                _kadastrDbProvider.Init();
                _loadedAlready.Wells = await _kadastrDbProvider.GetWells();
                _loadedAlready.Conpoints = await _kadastrDbProvider.GetConpoints();
                var count = _loadedAlready.Wells.Count;
                KadastrMessage = string.Format(Resources.SID_Nodes_loaded_from_Kadastr_so_far___0_, count);
                NotifyOfPropertyChange(nameof(IsStartEnabled));
                return true;
            }
            catch (Exception e)
            {
                KadastrMessage = Resources.SID_Kadastr_Db_connection_error___ + e.Message;
                return false;
            }
        }

        private async Task<bool> CheckDataCenter()
        {
            var isReady = await _desktopC2DWcfManager.CheckServerConnection(new CheckServerConnectionDto());
            if (!isReady)
            {
                ServerMessage = Resources.SID_DataCenter_connection_failed_;
            }
            else ServerMessage = Resources.SID_DataCenter_connected_successfully_;
            return isReady;
        }

        private bool _isFolderValid;
        private bool _isFree;

        public void SelectFolder()
        {
            var dlg = new OpenFileDialog
            {
                InitialDirectory = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory),
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            };
            if (dlg.ShowDialog() == true)
            {
                SelectedFolder = FileOperations.GetParentFolder(dlg.FileName);
                _isFolderValid = true;
                NotifyOfPropertyChange(nameof(IsStartEnabled));
            }
        }

        public void Start()
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += Bw_DoWork;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;

            IsFree = false;
            Mouse.OverrideCursor = Cursors.Wait;
            ProgressLines.Clear();
            bw.RunWorkerAsync();
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressLines.Add(Resources.SID_Done_);
            Mouse.OverrideCursor = null;
            IsFree = true;
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var st = (string)e.UserState;
            ProgressLines.Add(st);
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            _kadastrFilesParser.Run(SelectedFolder, worker);
        }

        public void Close()
        {
            TryClose();
        }
    }
}