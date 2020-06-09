﻿using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Web.Administration;

namespace Iit.Fibertest.UtilsLib
{
    public static class WebCommonOperation
    {
        public static void DeleteWebsite(string websiteName, BackgroundWorker worker)
        {
            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    var appPoll = serverManager.ApplicationPools.FirstOrDefault(p => p.Name == websiteName);
                    if (appPoll != null)
                        serverManager.ApplicationPools.Remove(appPoll);

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
    }
}