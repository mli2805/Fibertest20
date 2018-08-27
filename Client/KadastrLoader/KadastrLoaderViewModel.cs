using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
        private readonly IMyLog _logFile;
        private readonly LoadedAlready _loadedAlready;
        private readonly KadastrDbProvider _kadastrDbProvider;
        private readonly KadastrFilesParser _kadastrFilesParser;
        private readonly C2DWcfManager _c2DWcfManager;
        public string ServerIp { get; set; }

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

        public ObservableCollection<string> ProgressLines { get; set; } = new ObservableCollection<string>();

        public KadastrLoaderViewModel(IniFile iniFile, IMyLog logFile, LoadedAlready loadedAlready,
            KadastrDbProvider kadastrDbProvider,
            KadastrFilesParser kadastrFilesParser, C2DWcfManager c2DWcfManager)
        {
            _logFile = logFile;
            _loadedAlready = loadedAlready;
            _logFile.AssignFile("kadastr.log");
            _kadastrDbProvider = kadastrDbProvider;
            _kadastrFilesParser = kadastrFilesParser;
            _c2DWcfManager = c2DWcfManager;
            var serverAddresses = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            c2DWcfManager.SetServerAddresses(serverAddresses, "Kadastr", "");
            ServerIp = serverAddresses.Main.Ip4Address;
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
                ServerMessage = string.Format(Resources.SID_Nodes_loaded_from_Kadastr_so_far___0_, count);
                NotifyOfPropertyChange(nameof(IsStartEnabled));
                return true;
            }
            catch (Exception e)
            {
                ProgressLines.Add("Kadastr Db connection error!");
                ProgressLines.Add(e.Message);
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        private async Task<bool> CheckDataCenter()
        {
            var isReady = await _c2DWcfManager.CheckServerConnection(new CheckServerConnectionDto());
            if (!isReady)
            {
                ProgressLines.Add("DataCenter connection failed!");
                _logFile.AppendLine("DataCenter connection failed!");
            }
            else ProgressLines.Add("DataCenter connected successfully!");
            return isReady;
        }

        private bool _isFolderValid;
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

        public async void Start()
        {
            ProgressLines.Add("Started...");
            await _kadastrFilesParser.Go(SelectedFolder);
            ProgressLines.Add("Done.");
        }

        public void Close()
        {
            TryClose();
        }
    }
}