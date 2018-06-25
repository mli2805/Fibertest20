using System;
using System.Collections.ObjectModel;
using Iit.Fibertest.UtilsLib.ServiceManager;

namespace Iit.Fibertest.UtilsLib
{
    public static class ServiceOperations
    {
        public static bool InstallService(string serviceName, string serviceDisplayName,
            string serviceDescription, string filename, ObservableCollection<string> progressLines)
        {
            progressLines.Add($"{serviceDisplayName} service is being installed...");
            try
            {
                ServiceInstaller.Install(serviceName, serviceDisplayName, serviceDescription, filename);
            }
            catch (Exception)
            {
                progressLines.Add($"Cannot install service {serviceDisplayName}");
                return false;
            }

            progressLines.Add($"{serviceDisplayName} service installed successfully");
            return true;
        }

        public static bool UninstallServiceIfExist(string serviceName, string serviceDisplayName, ObservableCollection<string> progressLines)
        {
            if (!ServiceInstaller.ServiceIsInstalled(serviceName)) return true;

            if (!StopServiceIfRunning(serviceName, serviceDisplayName, progressLines)) return false;

            progressLines.Add($"{serviceDisplayName} service is being uninstalled...");
            ServiceInstaller.Uninstall(serviceName);
            if (ServiceInstaller.ServiceIsInstalled(serviceName) 
                && ServiceInstaller.ServiceIsInstalled(serviceName))
            {
                progressLines.Add($"Cannot uninstall service {serviceDisplayName}");
                return false;
            }

            progressLines.Add($"Service {serviceDisplayName} uninstalled successfully.");
            return true;
        }

        private static bool StopServiceIfRunning(string serviceName, string serviceDisplayName, ObservableCollection<string> progressLines)
        {
            if (ServiceInstaller.GetServiceStatus(serviceName) != ServiceState.Running) return true;

            progressLines.Add($"{serviceDisplayName} service is being stopped...");
            ServiceInstaller.StopService(serviceName);
            if (ServiceInstaller.GetServiceStatus(serviceName) != ServiceState.Stopped)
            {
                progressLines.Add($"Cannot stop service {serviceDisplayName}");
                return false;
            }

            progressLines.Add($"Service {serviceDisplayName} stopped successfully.");
            return true;
        }

    }
}