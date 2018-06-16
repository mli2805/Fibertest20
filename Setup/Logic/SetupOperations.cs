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
                    SetupDataCenter();
                    break;
                case InstallationType.Datacenter:
                    SetupDataCenter();
                    break;
                case InstallationType.RtuManager:
                    SetupDataCenter();
                    break;

            }
        }

        private void SetupDataCenter()
        {
            _progressLines.Add("Data Center setup started.");

            UninstallDcServiceIfNeeded();


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

            _progressLines.Add($"Service {DataCenterDisplayName} uninstalled succesfully.");
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
