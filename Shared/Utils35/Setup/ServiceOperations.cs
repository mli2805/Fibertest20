using System;
using System.ComponentModel;
using System.Linq;
using Iit.Fibertest.UtilsLib.ServiceManager;

namespace Iit.Fibertest.UtilsLib
{
    public static class ServiceOperations
    {
        public static bool InstallSericesOnPc(DestinationComputer dest, string installationFolder, BackgroundWorker worker)
        {
            return FtServices.List
                .Where(s => s.DestinationComputer == dest)
                .All(service => InstallService(service.Name, service.GetFullFilename(installationFolder), worker));
        }

        public static bool InstallService(string serviceName, string filename, BackgroundWorker worker)
        {
            var ftService = FtServices.List.First(s => s.Name == serviceName);
            worker.ReportProgress((int)BwReturnProgressCode.ServiceIsBeingInstalled, ftService.DisplayName);
            try
            {
                ServiceInstaller.Install(ftService.Name, ftService.DisplayName, ftService.Description, filename);
            }
            catch (Exception)
            {
                worker.ReportProgress((int)BwReturnProgressCode.CannotInstallService, ftService.DisplayName);
                return false;
            }

            worker.ReportProgress((int)BwReturnProgressCode.ServiceInstalledSuccessfully, ftService.DisplayName);
            return true;
        }

        public static bool UninstallAllServicesOnThisPc(BackgroundWorker worker)
        {
            foreach (var service in FtServices.List)
            {
                if (!UninstallServiceIfExist(service, worker))
                    return false;
            }

            return true;
        }

        private static bool UninstallServiceIfExist(FtService ftService, BackgroundWorker worker)
        {
            if (!ServiceInstaller.ServiceIsInstalled(ftService.Name)) return true;

            if (!StopServiceIfRunning(ftService.Name, ftService.DisplayName, worker)) return false;

            worker.ReportProgress((int)BwReturnProgressCode.ServiceIsBeingUninstalled, ftService.DisplayName);
            ServiceInstaller.Uninstall(ftService.Name);
            if (ServiceInstaller.ServiceIsInstalled(ftService.Name)
                && ServiceInstaller.ServiceIsInstalled(ftService.Name))
            {
                worker.ReportProgress((int)BwReturnProgressCode.CannotUninstallService, ftService.DisplayName);
                return false;
            }

            worker.ReportProgress((int)BwReturnProgressCode.ServiceUninstalledSuccessfully, ftService.DisplayName);
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