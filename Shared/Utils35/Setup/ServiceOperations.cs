using System;
using System.ComponentModel;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib.ServiceManager;

namespace Iit.Fibertest.UtilsLib
{
    public static class ServiceOperations
    {
        public static bool InstallService(string serviceName, string serviceDisplayName,
            string serviceDescription, string filename, BackgroundWorker worker)
        {
            worker.ReportProgress(0, string.Format(Resources.SID__0__service_is_being_installed___, serviceDisplayName));
            try
            {
                ServiceInstaller.Install(serviceName, serviceDisplayName, serviceDescription, filename);
            }
            catch (Exception)
            {
                worker.ReportProgress(0, string.Format(Resources.SID_Cannot_install_service__0_, serviceDisplayName));
                return false;
            }

            worker.ReportProgress(0, string.Format(Resources.SID__0__service_installed_successfully, serviceDisplayName));
            return true;
        }

        public static bool UninstallServiceIfExist(string serviceName, string serviceDisplayName, BackgroundWorker worker)
        {

            if (!ServiceInstaller.ServiceIsInstalled(serviceName)) return true;

            if (!StopServiceIfRunning(serviceName, serviceDisplayName, worker)) return false;

            worker.ReportProgress(0, string.Format(Resources.SID__0__service_is_being_uninstalled___, serviceDisplayName));
            ServiceInstaller.Uninstall(serviceName);
            if (ServiceInstaller.ServiceIsInstalled(serviceName) 
                && ServiceInstaller.ServiceIsInstalled(serviceName))
            {
                worker.ReportProgress(0, string.Format(Resources.SID_Cannot_uninstall_service__0_, serviceDisplayName));
                return false;
            }

            worker.ReportProgress(0, string.Format(Resources.SID_Service__0__uninstalled_successfully_, serviceDisplayName));
            return true;
        }

        private static bool StopServiceIfRunning(string serviceName, string serviceDisplayName, BackgroundWorker worker)
        {
            if (ServiceInstaller.GetServiceStatus(serviceName) != ServiceState.Running) return true;

            worker.ReportProgress(0, string.Format(Resources.SID__0__service_is_being_stopped___, serviceDisplayName));
            ServiceInstaller.StopService(serviceName);
            if (ServiceInstaller.GetServiceStatus(serviceName) != ServiceState.Stopped)
            {
                worker.ReportProgress(0, string.Format(Resources.SID_Cannot_stop_service__0_, serviceDisplayName));
                return false;
            }

            worker.ReportProgress(0, string.Format(Resources.SID_Service__0__stopped_successfully_, serviceDisplayName));
            return true;
        }

    }
}