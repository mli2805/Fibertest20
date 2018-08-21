using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Microsoft.Win32;

namespace KadastrLoader
{
    public class KadastrLoaderViewModel : Screen, IShell
    {
        private readonly KadastrDbSettings _kadastrDbSettings;
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

        public KadastrLoaderViewModel(IniFile iniFile, KadastrDbSettings kadastrDbSettings)
        {
            _kadastrDbSettings = kadastrDbSettings;
            var serverAddresses = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            ServerIp = serverAddresses.Main.Ip4Address;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Load_from_Kadastr;
        }

        private bool _isDbReady;
        public void ConnectKadastrDb()
        {
            _kadastrDbSettings.Init();
            using (var dbContext = new KadastrDbContext(_kadastrDbSettings.Options))
            {
                dbContext.Database.EnsureCreated();
                var count = dbContext.Wells.Count();
                ServerMessage = $"Loaded from Kadastr so far: {count}";
                _isDbReady = true;
                NotifyOfPropertyChange(nameof(IsStartEnabled)); }

           
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

        public void Close()
        {
            TryClose();
        }
    }
}