using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.Win32;

namespace DbMigrationWpf
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IniFile _iniFile;
        private readonly LogFile _logFile;
        private NetAddress _clientAddress;
        private DoubleAddress _serverDoubleAddress;
        private readonly DesktopC2DWcfManager _desktopC2DWcfManager;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly GraphModel _graphModel = new GraphModel();


        private string _ft20ServerAddress;
        public string Ft20ServerAddress 
        { 
            get => _ft20ServerAddress;
            set
            {
                if (value == _ft20ServerAddress) return;
                _ft20ServerAddress = value;
                NotifyOfPropertyChange();
                _serverDoubleAddress.Main.Ip4Address = _ft20ServerAddress;
                _iniFile.WriteServerAddresses(_serverDoubleAddress);
                _desktopC2DWcfManager.SetServerAddresses(_serverDoubleAddress, @"migrator", _clientAddress.Ip4Address);
            }
        }

        private string _ft15ServerAddress;
        public string Ft15ServerAddress
        {
            get => _ft15ServerAddress;
            set
            {
                if (value == _ft15ServerAddress) return;
                _ft15ServerAddress = value;
                NotifyOfPropertyChange();
            }
        }

        public int OldMySqlPort { get; set; } = 3306;
        public int NewMySqlPort { get; set; } = 33060;

        public bool HasKadastr { get;set; }

        public ObservableCollection<string> ProgressLines { get; set; } = new ObservableCollection<string>();

        private string _currentLicenseText = "";
        public string CurrentLicenseText
        {
            get => _currentLicenseText;
            set
            {
                if (Equals(value, _currentLicenseText)) return;
                _currentLicenseText = value;
                NotifyOfPropertyChange();
            }
        }

        private string _exportFileName;
        public string ExportFileName
        {
            get => _exportFileName;
            set
            {
                if (Equals(value, _exportFileName)) return;
                _exportFileName = value;
                NotifyOfPropertyChange();
            }
        }

        public ShellViewModel()
        {
            _iniFile = new IniFile();
            _iniFile.AssignFile("migrator.ini");
            _logFile = new LogFile(_iniFile, 50000);
            _logFile.AssignFile("migrator.log");

            _desktopC2DWcfManager = new DesktopC2DWcfManager(_iniFile, _logFile);
            _commonC2DWcfManager = new CommonC2DWcfManager(_iniFile, _logFile);

            _clientAddress = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);
            if (_clientAddress.Ip4Address == "0.0.0.0")
            {
                _clientAddress.Ip4Address = LocalAddressResearcher.GetAllLocalAddresses().First();
                _iniFile.Write(IniSection.ClientLocalAddress, IniKey.Ip, _clientAddress.Ip4Address);
            }

            _serverDoubleAddress = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToDesktopClient);
            if (_serverDoubleAddress.Main.Ip4Address == "0.0.0.0")
            {
                _serverDoubleAddress.Main = new NetAddress(_clientAddress.Ip4Address, TcpPorts.ServerListenToDesktopClient);
                _iniFile.WriteServerAddresses(_serverDoubleAddress);
            }
            _desktopC2DWcfManager.SetServerAddresses(_serverDoubleAddress, @"migrator", _clientAddress.Ip4Address);
            _commonC2DWcfManager.SetServerAddresses(_serverDoubleAddress, @"migrator", _clientAddress.Ip4Address);

            _ft20ServerAddress = _serverDoubleAddress.Main.Ip4Address;
            _ft15ServerAddress = _iniFile.Read(IniSection.Migrator, IniKey.OldFibertestServerIp, "0.0.0.0");
            if (_ft15ServerAddress == "0.0.0.0")
            {
                _ft15ServerAddress = _clientAddress.Ip4Address;
                _iniFile.Write(IniSection.Migrator, IniKey.OldFibertestServerIp, _ft15ServerAddress);
            }

            _exportFileName = @"..\Db\export.txt";
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Migrate Fibertest_1.5 Db to Fibertest_2.0";
        }

        public void ChooseExportFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                ExportFileName = openFileDialog.FileName;
            }
        }
        public async void LoadLicense()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location);

            var licManager = new LicenseManager();
            var licenseInFile = licManager.ReadLicenseFromFile(path);
            if (licenseInFile == null) return;
            var cmd = new ApplyLicense()
            {
                LicenseId = licenseInFile.LicenseId,
                Owner = licenseInFile.Owner,
                RtuCount = new LicenseParameter(licenseInFile.RtuCount),
                ClientStationCount = new LicenseParameter(licenseInFile.ClientStationCount),
                SuperClientStationCount = new LicenseParameter(licenseInFile.SuperClientStationCount),
                Version = licenseInFile.Version,
            };

            var result = await _desktopC2DWcfManager.SendCommandAsObj(cmd);
            if (result != null)
            {
                CurrentLicenseText = result;
                return;
            }

            CurrentLicenseText = $"Licensed RTU count - {licenseInFile.RtuCount.Value}";
        }

        public async void Migrate()
        {
            var migrationManager = new MigrationManager(_logFile, _graphModel, _desktopC2DWcfManager, _commonC2DWcfManager, ProgressLines);
            await migrationManager.Migrate(ExportFileName, Ft15ServerAddress, OldMySqlPort, Ft20ServerAddress, NewMySqlPort, HasKadastr);
            File.WriteAllLines(@"..\log\progress.txt", ProgressLines);
        }

        public void Close()
        {
            TryClose();
        }
    }
}