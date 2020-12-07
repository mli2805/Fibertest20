using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Iit.Fibertest.UtilsLib.ServiceManager;

namespace Iit.Fibertest.UtilsLib
{
    public class FtService
    {
        public readonly string Name;
        public readonly string DisplayName;
        public string Description => DisplayName;

        public FtService(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }
    }

    public class FtServices
    {
        public static List<FtService> List = new List<FtService>
        {
            new FtService("FibertestDcService", "Fibertest 2.0 DataCenter Server Service"), 
            new FtService("FibertestWaService", "Fibertest 2.0 DataCenter WebApi Service"),
            new FtService("FibertestRtuWatchdog", "Fibertest 2.0 RTU Watchdog Service"),
            new FtService("FibertestRtuService", "Fibertest 2.0 RTU Manager Service"),
        };
    }

    public static class ServiceOperations
    {
        public static FtServices FtServices;

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

        public static bool UninstallServiceIfExist(string serviceName, BackgroundWorker worker)
        {
            var ftService = FtServices.List.First(s => s.Name == serviceName);

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