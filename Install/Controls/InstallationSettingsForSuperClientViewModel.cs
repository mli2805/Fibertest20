using System.IO;
using Caliburn.Micro;
using System.Windows;
using Iit.Fibertest.UtilsLib;
using System.Diagnostics;

namespace Iit.Fibertest.Install
{
    public class InstallationSettingsForSuperClientViewModel : PropertyChangedBase
    {
        private readonly CurrentInstallation _currentInstallation;
        private Visibility _visibility = Visibility.Collapsed;

        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }

        public string ClientVersion { get; set; } = string.Empty;


        private string _r0;
        public string R0
        {
            get => _r0;
            set
            {
                if (value == _r0) return;
                _r0 = value;
                NotifyOfPropertyChange();
            }
        }

        public string R1 => "Reinstall";
        public string R2 => "Install in a new folder";

        public bool IsR1Checked { get; set; } = true;

        public InstallationSettingsForSuperClientViewModel(CurrentInstallation currentInstallation)
        {
            _currentInstallation = currentInstallation;
        }

        public bool CheckCurrentClientVersion()
        {
            var clientFolder = _currentInstallation.InstallationFolder + @"Client\";
            if (!Directory.Exists(clientFolder))
                return false;

            var clientExe = clientFolder + @"bin\Iit.Fibertest.Client.exe";
            if (!File.Exists(clientExe))
                return false;

            FileVersionInfo vi = FileVersionInfo.GetVersionInfo(clientExe);
            ClientVersion = vi.FileVersion;
            R0 = $"Client {ClientVersion} is installed";
            return true;
        }
    }
}
