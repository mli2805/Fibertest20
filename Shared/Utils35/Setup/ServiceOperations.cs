using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Iit.Fibertest.UtilsLib.ServiceManager;

namespace Iit.Fibertest.UtilsLib
{
    public static class ServiceOperations
    {
        public static bool InstallService(string serviceName, string serviceDisplayName,
            string serviceDescription, string filename, BackgroundWorker worker)
        {
            worker.ReportProgress(0, $"{serviceDisplayName} service is being installed...");
            try
            {
                ServiceInstaller.Install(serviceName, serviceDisplayName, serviceDescription, filename);
            }
            catch (Exception)
            {
                worker.ReportProgress(0, $"Cannot install service {serviceDisplayName}");
                return false;
            }

            worker.ReportProgress(0, $"{serviceDisplayName} service installed successfully");
            return true;
        }

        public static bool UninstallServiceIfExist(string serviceName, string serviceDisplayName, BackgroundWorker worker)
        {
            if (!ServiceInstaller.ServiceIsInstalled(serviceName)) return true;

            if (!StopServiceIfRunning(serviceName, serviceDisplayName, worker)) return false;

            worker.ReportProgress(0, $"{serviceDisplayName} service is being uninstalled...");
            ServiceInstaller.Uninstall(serviceName);
            if (ServiceInstaller.ServiceIsInstalled(serviceName) 
                && ServiceInstaller.ServiceIsInstalled(serviceName))
            {
                worker.ReportProgress(0, $"Cannot uninstall service {serviceDisplayName}");
                return false;
            }

            worker.ReportProgress(0, $"Service {serviceDisplayName} uninstalled successfully.");
            return true;
        }

        private static bool StopServiceIfRunning(string serviceName, string serviceDisplayName, BackgroundWorker worker)
        {
            if (ServiceInstaller.GetServiceStatus(serviceName) != ServiceState.Running) return true;

            worker.ReportProgress(0, $"{serviceDisplayName} service is being stopped...");
            ServiceInstaller.StopService(serviceName);
            if (ServiceInstaller.GetServiceStatus(serviceName) != ServiceState.Stopped)
            {
                worker.ReportProgress(0, $"Cannot stop service {serviceDisplayName}");
                return false;
            }

            worker.ReportProgress(0, $"Service {serviceDisplayName} stopped successfully.");
            return true;
        }

    }
}