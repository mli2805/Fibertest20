﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Iit.Fibertest.UtilsLib;
using Microsoft.Web.Administration;

namespace Iit.Fibertest.Install
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
        /// <param name="certificateName">Friendly name of SSL certificate</param>
        /// <param name="path">d:\\MySite</param>
        /// <param name="worker"></param>
        public static void CreateWebsite(string websiteName, string bindingProtocol, string bindingInformation,
            string certificateName, string path, BackgroundWorker worker)
        {
            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    if (serverManager.ApplicationPools.FirstOrDefault(p => p.Name == websiteName) == null)
                    {
                        ApplicationPool newPool = serverManager.ApplicationPools.Add(websiteName);  
                        newPool.ProcessModel.IdleTimeout = TimeSpan.Zero;  
                        newPool.AutoStart = true;
                        var attr = newPool.Attributes.FirstOrDefault(a => a.Name == "startMode");
                        if (attr != null) attr.Value = 1; // OnDemand = 0;   AlwaysRunning = 1
                        serverManager.CommitChanges();  
                    }

                    Site site;
                    if (!string.IsNullOrEmpty(certificateName))
                    {
                        var certificate = FindCertificate(certificateName);
                        if (certificate == null)
                            throw new Exception($"Cannot find certificate with name {certificateName}");
                        site = serverManager.Sites.Add(websiteName, bindingInformation, path, certificate.GetCertHash());
                    }
                    else
                        site = serverManager.Sites.Add(websiteName, bindingProtocol, bindingInformation, path);

                    site.ApplicationDefaults.ApplicationPoolName = websiteName;

                    serverManager.CommitChanges();
                }

                worker.ReportProgress((int)BwReturnProgressCode.SiteInstalledSuccessfully, websiteName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                worker.ReportProgress((int)BwReturnProgressCode.SiteInstallationError, websiteName + "; " + e.Message);
            }
        }

        private static X509Certificate2 FindCertificate(string name)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
            X509Certificate2 result = null;
            foreach (var cert in store.Certificates)
                if (cert.FriendlyName == name)
                    result = cert;
            store.Close();
            return result;
        }

        public static IEnumerable<string> GetCertificates()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);

            foreach (var cert in store.Certificates)
                yield return cert.FriendlyName;
            store.Close();
        }


//        public static void DeleteWebsite(string websiteName, BackgroundWorker worker)
//        {
//            try
//            {
//                using (ServerManager serverManager = new ServerManager())
//                {
//                    Site site = serverManager.Sites[websiteName];
//                    if (site == null) return;
//                    serverManager.Sites.Remove(site);
//                    serverManager.CommitChanges();
//                    worker.ReportProgress((int)BwReturnProgressCode.SiteUninstalledSuccessfully, websiteName);
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.Message);
//                worker.ReportProgress((int)BwReturnProgressCode.SiteUninstallationError, websiteName + "; " + e.Message);
//            }
//        }

//        public static bool StopWebsite(string websiteName)
//        {
//            var iisManager = new ServerManager();
//            var site = iisManager.Sites.FirstOrDefault(s => s.Name == websiteName);
//            if (site == null) return true;
//            site.Stop();
//            iisManager.CommitChanges();
//            return site.State == ObjectState.Stopped;
//        }
//
//        public static bool StartWebsite(string websiteName)
//        {
//            var iisManager = new ServerManager();
//            var site = iisManager.Sites.FirstOrDefault(s => s.Name == websiteName);
//            if (site == null) return true;
//            site.Start();
//            iisManager.CommitChanges();
//            return site.State == ObjectState.Started;
//        }
    }
}