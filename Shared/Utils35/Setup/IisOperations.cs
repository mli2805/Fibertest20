using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Web.Administration;

namespace Iit.Fibertest.UtilsLib
{
    public static class IisOperations
    {
        public static bool DoesWebsiteExist(string websiteName)
        {
            var iisManager = new ServerManager();
            return iisManager.Sites.Any(s => s.Name == websiteName);
        }

        /// <summary>
        /// creates and at once starts web site
        /// </summary>
        /// <param name="websiteName">fibertest_angular_app</param>
        /// <param name="bindingProtocol">http</param>
        /// <param name="bindingInformation">*:8080:</param>
        /// <param name="path">d:\\MySite</param>
        /// <param name="worker"></param>
        public static void CreateWebsite(string websiteName, string bindingProtocol, string bindingInformation,
            string path, BackgroundWorker worker)
        {
            try
            {
                ServerManager iisManager = new ServerManager();
                iisManager.Sites.Add(websiteName, bindingProtocol, bindingInformation, path);
                iisManager.CommitChanges();
                worker.ReportProgress((int)BwReturnProgressCode.SiteInstalledSuccessfully, websiteName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                worker.ReportProgress((int)BwReturnProgressCode.SiteInstallationError, websiteName + "; " + e.Message);

            }
        }

        public static void DeleteWebsite(string websiteName, BackgroundWorker worker)
        {
            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    Site site = serverManager.Sites[websiteName];
                    if (site == null) return;
                    serverManager.Sites.Remove(site);
                    serverManager.CommitChanges();
                    worker.ReportProgress((int)BwReturnProgressCode.SiteUninstalledSuccessfully, websiteName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                worker.ReportProgress((int)BwReturnProgressCode.SiteUninstallationError, websiteName + "; " + e.Message);
            }
        }

        public static bool StopWebsite(string websiteName)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites.FirstOrDefault(s => s.Name == websiteName);
            if (site == null) return true;
            site.Stop();
            iisManager.CommitChanges();
            return site.State == ObjectState.Stopped;
        }

        public static bool StartWebsite(string websiteName)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites.FirstOrDefault(s => s.Name == websiteName);
            if (site == null) return true;
            site.Start();
            iisManager.CommitChanges();
            return site.State == ObjectState.Started;
        }
    }
}