using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly KadastrDbSettings _kadastrDbSettings;
        private readonly KadastrFilesParser _kadastrFilesParser;
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

        public bool IsStartEnabled => _isDbReady && _isFolderValid;

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

        public KadastrLoaderViewModel(IniFile iniFile, IMyLog logFile, 
            KadastrDbSettings kadastrDbSettings, KadastrFilesParser kadastrFilesParser,
            C2DWcfManager c2DWcfManager)
        {
            _logFile = logFile;
            _logFile.AssignFile("kadastr.log");
            _kadastrDbSettings = kadastrDbSettings;
            _kadastrFilesParser = kadastrFilesParser;
            var serverAddresses = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            c2DWcfManager.SetServerAddresses(serverAddresses, "Kadastr", "");
            ServerIp = serverAddresses.Main.Ip4Address;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Load_from_Kadastr;
        }

        private bool _isDbReady;
        public void ConnectKadastrDb()
        {
            try
            {
                _kadastrDbSettings.Init();
                using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
                {
                    dbContext.Database.EnsureCreated();
                    var count = dbContext.Wells.Count();
                    ServerMessage = $"Loaded from Kadastr so far: {count}";
                    _isDbReady = true;
                    NotifyOfPropertyChange(nameof(IsStartEnabled));
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                throw;
            }
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