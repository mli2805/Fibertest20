using System.Collections.ObjectModel;

namespace Setup
{
    public class SetupDatacenterOperations
    {
        private const string DataCenterServiceName = "FibertestDcService";
        private const string DataCenterDisplayName = "Fibertest 2.0 DataCenter Server";


        public void SetupDataCenter(ObservableCollection<string> progressLines)
        {
            progressLines.Add("Data Center setup started.");
            if (!UninstallDcServiceIfNeeded(progressLines))
                return;


        }

        private bool UninstallDcServiceIfNeeded(ObservableCollection<string> progressLines)
        {
            if (!ServiceInstaller.ServiceIsInstalled(DataCenterServiceName)) return true;

            if (!StopServiceIfRunning(progressLines)) return false;

            progressLines.Add("Data Center service uninstalling...");
            ServiceInstaller.Uninstall(DataCenterServiceName);
            if (ServiceInstaller.ServiceIsInstalled(DataCenterServiceName))
            {
                progressLines.Add($"Cannot uninstall service {DataCenterDisplayName}");
                return false;
            }

            progressLines.Add($"Service {DataCenterDisplayName} uninstalled successfully.");
            return true;
        }

        private bool StopServiceIfRunning(ObservableCollection<string> progressLines)
        {
            if (ServiceInstaller.GetServiceStatus(DataCenterServiceName) != ServiceState.Running) return true;

            progressLines.Add("Data Center service stopping...");
            ServiceInstaller.StopService(DataCenterServiceName);
            if (ServiceInstaller.GetServiceStatus(DataCenterServiceName) != ServiceState.Stopped)
            {
                progressLines.Add($"Cannot stop service {DataCenterDisplayName}");
                return false;
            }

            progressLines.Add($"Service {DataCenterDisplayName} stopped successfully.");
            return true;
        }

    }
}