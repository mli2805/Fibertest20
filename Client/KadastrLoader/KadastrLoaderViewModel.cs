using System;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Microsoft.Win32;

namespace KadastrLoader 
{
    public class KadastrLoaderViewModel : Screen, IShell
    {
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

        private bool _isStartEnabled;
        public bool IsStartEnabled
        {
            get => _isStartEnabled;
            set
            {
                if (value == _isStartEnabled) return;
                _isStartEnabled = value;
                NotifyOfPropertyChange();
            }
        }

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

        public KadastrLoaderViewModel(IniFile iniFile)
        {
            var serverAddresses = iniFile.ReadDoubleAddress((int) TcpPorts.ServerListenToClient);
            ServerIp = serverAddresses.Main.Ip4Address;
            CreateKadastrDbIfNeeded();
        }

        private void CreateKadastrDbIfNeeded()
        {

        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Load_from_Kadastr;
        }

        public void ConnectKadastrDb()
        {
            ServerMessage = "Kadastr DB contains: ";
            IsStartEnabled = true;
        }

        public void SelectFolder()
        {
            var dlg = new OpenFileDialog
            {
                InitialDirectory = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory),
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            };
            if (dlg.ShowDialog() == true)
                SelectedFolder = FileOperations.GetParentFolder(dlg.FileName);
        }

        public void Close()
        {
            TryClose();
        }
    }
}