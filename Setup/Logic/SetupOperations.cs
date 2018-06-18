using System.Collections.ObjectModel;

namespace Setup
{
    public class SetupOperations
    {
        private const string DataCenterServiceName = "FibertestDcService";
        private const string DataCenterDisplayName = "Fibertest 2.0 DataCenter Server";

        private readonly CurrentInstallation _currentInstallation;
        private ObservableCollection<string> _progressLines;

        public SetupOperations(CurrentInstallation currentInstallation)
        {
            _currentInstallation = currentInstallation;
        }

        public void Run(ObservableCollection<string> progressLines)
        {
            _progressLines = progressLines;

            switch (_currentInstallation.InstallationType)
            {
                case InstallationType.Client:
                    SetupClient();
                    break;
                case InstallationType.Datacenter:
                    SetupDataCenter();
                    break;
                case InstallationType.RtuManager:
                    SetupRtuManager();
                    break;

            }
        }

        private void SetupClient()
        {
            _progressLines.Add("Client setup started.");
        }

        private void SetupDataCenter()
        {
            _progressLines.Add("Data Center setup started.");

            UninstallDcServiceIfNeeded();
        }

        private void SetupRtuManager()
        {
            _progressLines.Add("RTU Manager setup started.");
        }

        private void UninstallDcServiceIfNeeded()
        {
            if (!ServiceInstaller.ServiceIsInstalled(DataCenterServiceName)) return;

            if (!StopServiceIfRunning()) return;

            _progressLines.Add("Data Center service uninstalling...");
            ServiceInstaller.Uninstall(DataCenterServiceName);
            if (ServiceInstaller.ServiceIsInstalled(DataCenterServiceName))
            {
                _progressLines.Add($"Cannot uninstall service {DataCenterDisplayName}");
                return;
            }

            _progressLines.Add($"Service {DataCenterDisplayName} uninstalled successfully.");
        }

        private bool StopServiceIfRunning()
        {
            if (ServiceInstaller.GetServiceStatus(DataCenterServiceName) != ServiceState.Running) return true;

            _progressLines.Add("Data Center service stopping...");
            ServiceInstaller.StopService(DataCenterServiceName);
            if (ServiceInstaller.GetServiceStatus(DataCenterServiceName) != ServiceState.Stopped)
            {
                _progressLines.Add($"Cannot stop service {DataCenterDisplayName}");
                return false;
            }

            _progressLines.Add($"Service {DataCenterDisplayName} stopped succesfully.");
            return true;
        }
    }
}
