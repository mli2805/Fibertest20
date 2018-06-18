using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Setup
{
    public class SetupDatacenterOperations
    {
        private const string DataCenterServiceName = "FibertestDcService";
        private const string DataCenterDisplayName = "Fibertest 2.0 DataCenter Server";

        private const string SourcePathDatacenter = @"..\DcFiles";
        private const string TargetPathDatacenter = @"C:\IIT-Fibertest\DataCenter\bin";

        public bool SetupDataCenter(ObservableCollection<string> progressLines)
        {
            progressLines.Add("Data Center setup started.");
            if (!UninstallDcServiceIfNeeded(progressLines))
                return false;

            if (!SetupOperations.DirectoryCopyWithDecorations(SourcePathDatacenter, TargetPathDatacenter, progressLines))
                return false;

            if (!InstallService(progressLines)) return false;

            return true;
        }

        private bool InstallService(ObservableCollection<string> progressLines)
        {
            var filename = Path.Combine(TargetPathDatacenter, @"Iit.Fibertest.DataCenterService.exe");
            progressLines.Add("Data Center service is being installed...");
            try
            {
                ServiceInstaller.Install(DataCenterServiceName, DataCenterDisplayName, filename);
            }
            catch (Exception)
            {
                progressLines.Add($"Cannot install service {DataCenterDisplayName}");
                return false;
            }

            progressLines.Add("Data Center service installed successfully");
            return true;
        }

        private bool UninstallDcServiceIfNeeded(ObservableCollection<string> progressLines)
        {
            if (!ServiceInstaller.ServiceIsInstalled(DataCenterServiceName)) return true;

            if (!StopServiceIfRunning(progressLines)) return false;

            progressLines.Add("Data Center service is being uninstalled...");
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

            progressLines.Add("Data Center service is being stopped...");
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