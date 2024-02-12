using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.InstallLib
{
    public static class ServiceOperations
    {
        public static bool InstallSericesOnPc(DestinationComputer dest, string installationFolder, BackgroundWorker worker)
        {
            return FtServices.List
                .Where(s => s.DestinationComputer == dest)
                .All(service => InstallService(service, installationFolder, worker));
        }

        public static bool InstallService(FtService ftService, string installationFolder, BackgroundWorker worker)
        {
            worker.ReportProgress((int)BwReturnProgressCode.ServiceIsBeingInstalled, ftService.DisplayName);
            try
            {
                ServiceInstaller.Install(ftService.Name, ftService.DisplayName, ftService.Description, ftService.GetFullFilename(installationFolder));
                if (ftService.Name == "FibertestRtuService")
                    RegistryOperations.SetRestartAsServiceFailureActions(ftService.Name);
            }
            catch (Exception)
            {
                worker.ReportProgress((int)BwReturnProgressCode.CannotInstallService, ftService.DisplayName);
                return false;
            }

            worker.ReportProgress((int)BwReturnProgressCode.ServiceInstalledSuccessfully, ftService.DisplayName);
            return true;
        }

        public static bool UninstallServiceIfExist(FtService ftService, BackgroundWorker worker)
        {
            if (!ServiceInstaller.ServiceIsInstalled(ftService.Name)) return true;

            if (!StopServiceIfRunning(ftService.Name, ftService.DisplayName, worker)) return false;

            worker.ReportProgress((int)BwReturnProgressCode.ServiceIsBeingUninstalled, ftService.DisplayName);
            ServiceInstaller.Uninstall(ftService.Name);
            if (ServiceInstaller.ServiceIsInstalled(ftService.Name)) 
                Thread.Sleep(2000);
            if (ServiceInstaller.ServiceIsInstalled(ftService.Name))
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