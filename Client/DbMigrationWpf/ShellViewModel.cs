using System.Collections.ObjectModel;
using System.IO;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace DbMigrationWpf
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IniFile _iniFile;
        private readonly LogFile _logFile;
        private DoubleAddress _serverDoubleAddress;
        private readonly C2DWcfManager _c2DWcfManager;
        private readonly GraphModel _graphModel = new GraphModel();


        public string Ft20ServerAddress { get; set; }
        public string Ft15ServerAddress { get; set; }

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
                NotifyOfPropertyChange(nameof(IsAllReadyForMigration));
            }
        }

        public bool IsAllReadyForMigration
        {
            get => CurrentLicenseText != "";
            
        }

        public ShellViewModel()
        {
            _iniFile = new IniFile();
            _iniFile.AssignFile("migrator.ini");
            _logFile = new LogFile(_iniFile);
            _logFile.AssignFile("migrator.log");

            _c2DWcfManager = new C2DWcfManager(_iniFile, _logFile);
            NetAddress clientAddress = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);
            _serverDoubleAddress = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            _c2DWcfManager.SetServerAddresses(_serverDoubleAddress, @"migrator", clientAddress.Ip4Address);

            Initialize();
        }

        private void Initialize()
        {
            Ft20ServerAddress = _serverDoubleAddress.Main.Ip4Address;
            Ft15ServerAddress = _iniFile.Read(IniSection.Migrator, IniKey.OldFibertestServerIp, "0.0.0.0");
        }

        public async void LoadLicense()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location);

            var licManager = new LicenseManager();
            var license = licManager.ReadLicenseFromFile(path);
            if (license == null) return;
            var cmd = new ApplyLicense()
            {
                Owner = license.Owner,
                RtuCount = license.RtuCount,
                ClientStationCount = license.ClientStationCount,
                SuperClientEnabled = license.SuperClientEnabled,
                Version = license.Version,
            };
            var result = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (result != null)
            {
                CurrentLicenseText = result;
                return;
            }

            CurrentLicenseText = $"RTU - {license.RtuCount}";
        }

        public async void Migrate()
        {
            var migrationManager = new MigrationManager(_logFile, _graphModel, _c2DWcfManager, ProgressLines);
            await migrationManager.Migrate(false, Ft15ServerAddress);
            File.WriteAllLines(@"..\log\progress.txt", ProgressLines);
        }

        public void Close()
        {
            TryClose();
        }
    }
}