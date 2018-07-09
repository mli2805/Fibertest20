using System;
using System.ComponentModel;
using Iit.Fibertest.UtilsLib.ServiceManager;

namespace Iit.Fibertest.UtilsLib
{
    public static class ServiceOperations
    {
        public static bool InstallService(string serviceName, string serviceDisplayName,
            string serviceDescription, string filename, BackgroundWorker worker)
        {
            worker.ReportProgress((int)BwReturnProgressCode.ServiceIsBeingInstalled, serviceDisplayName);
            try
            {
                ServiceInstaller.Install(serviceName, serviceDisplayName, serviceDescription, filename);
            }
            catch (Exception)
            {
                worker.ReportProgress((int)BwReturnProgressCode.CannotInstallService, serviceDisplayName);
                return false;
            }

            worker.ReportProgress((int)BwReturnProgressCode.ServiceInstalledSuccessfully, serviceDisplayName);
            return true;
        }

        public static bool UninstallServiceIfExist(string serviceName, string serviceDisplayName, BackgroundWorker worker)
        {

            if (!ServiceInstaller.ServiceIsInstalled(serviceName)) return true;

            if (!StopServiceIfRunning(serviceName, serviceDisplayName, worker)) return false;

            worker.ReportProgress((int)BwReturnProgressCode.ServiceIsBeingUninstalled, serviceDisplayName);
            ServiceInstaller.Uninstall(serviceName);
            if (ServiceInstaller.ServiceIsInstalled(serviceName) 
                && ServiceInstaller.ServiceIsInstalled(serviceName))
            {
                worker.ReportProgress((int)BwReturnProgressCode.CannotUninstallService, serviceDisplayName);
                return false;
            }

            worker.ReportProgress((int)BwReturnProgressCode.ServiceUninstalledSuccessfully, serviceDisplayName);
            return true;
        }

        private static bool StopServiceIfRunning(string serviceName, string serviceDisplayName, BackgroundWorker worker)
        {
            if (ServiceInstaller.GetServiceStatus(serviceName) != ServiceState.Running) return true;

            worker.ReportProgress((int)BwReturnProgressCode.ServiceIsBeingStopped, serviceDisplayName);
            ServiceInstaller.StopService(serviceName);
            if (ServiceInstaller.GetServiceStatus(serviceName) != ServiceState.Stopped)
            {
                worker.ReportProgress((int)BwReturnProgressCode.CannotStopService, serviceDisplayName);
                return false;
            }

            worker.ReportProgress((int)BwReturnProgressCode.ServiceStoppedSuccessfully, serviceDisplayName);
            return true;
        }

    }
}