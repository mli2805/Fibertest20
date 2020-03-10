﻿using System;
using System.ComponentModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class SetupDataCenterOperations
    {
        private const string DataCenterServiceName = "FibertestDcService";
        private const string DataCenterDisplayName = "Fibertest 2.0 DataCenter Server";
        private const string DataCenterServiceDescription = "Fibertest 2.0 DataCenter Server Service";

        private const string WebApiSiteName = "fibertest_web_api";
        private const string WebClientSiteName = "fibertest_web_client";

        private const string SourcePathDataCenter = @"..\DcFiles";
        private const string DataCenterSubdir = @"DataCenter\bin";
        private const string ServiceFilename = @"Iit.Fibertest.DataCenterService.exe";

        private const string SourcePathWebApi = @"..\WebApi";
        private const string WebApiSubdir = @"WebApi\publish";
        private const string SourcePathWebClient = @"..\WebClient";
        private const string WebClientSubdir = @"WebClient";

        public bool SetupDataCenter(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            if (!SetupDataCenterComponent(worker, currentInstallation)) return false;

            if (currentInstallation.IsWebNeeded)
            {
                if (!SetupWebComponents(worker, currentInstallation)) return false;
            }
            return true;
        }

        private static bool SetupDataCenterComponent(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            var fullDataCenterPath = Path.Combine(currentInstallation.InstallationFolder, DataCenterSubdir);

            worker.ReportProgress((int)BwReturnProgressCode.DataCenterSetupStarted);
            if (!ServiceOperations.UninstallServiceIfExist(DataCenterServiceName, DataCenterDisplayName, worker))
                return false;

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopied);
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathDataCenter,
                fullDataCenterPath, worker))
                return false;
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);

            IniOperations.SaveMysqlTcpPort(currentInstallation.InstallationFolder, currentInstallation.MySqlTcpPort);

            var filename = Path.Combine(fullDataCenterPath, ServiceFilename);
            if (!ServiceOperations.InstallService(DataCenterServiceName,
                DataCenterDisplayName, DataCenterServiceDescription, filename, worker)) return false;

            worker.ReportProgress((int)BwReturnProgressCode.DataCenterSetupCompletedSuccessfully);
            return true;
        }

        private static bool SetupWebComponents(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.WebComponentsSetupStarted);

            if (!DeleteExistingWebSites(worker, currentInstallation))
                return false;

            if (!CopyWebComponents(worker, currentInstallation))
                return false;

            // WebApi always HTTPS regardless of WebClient (angular app) 
            IisOperations.CreateWebsite(WebApiSiteName, "https", "*:11080:",
                Path.Combine(currentInstallation.InstallationFolder, WebApiSubdir), worker);

            // WebClient site name is always the same regardless of protocol (http or https)
            if (currentInstallation.IsWebByHttps)
                IisOperations.CreateWebsite(WebClientSiteName, "https", "*:443:",
                    Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir), worker);
            else
                IisOperations.CreateWebsite(WebClientSiteName, "http", "*:80:",
                    Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir), worker);

            worker.ReportProgress((int)BwReturnProgressCode.WebComponentsSetupCompletedSuccessfully);
            return true;
        }

        private static bool DeleteExistingWebSites(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            try
            {
                if (IisOperations.DoesWebsiteExist(WebApiSiteName))
                {
                    IisOperations.DeleteWebsite(WebApiSiteName, worker);
                    var webApiSitePath = Path.Combine(currentInstallation.InstallationFolder, WebApiSubdir);
                    if (Directory.Exists(webApiSitePath))
                        Directory.Delete(webApiSitePath, true);
                }

                if (IisOperations.DoesWebsiteExist(WebClientSiteName))
                {
                    IisOperations.DeleteWebsite(WebClientSiteName, worker);
                    var webClientSitePath = Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir);
                    if (Directory.Exists(webClientSitePath))
                        Directory.Delete(webClientSitePath, true);
                }
            }
            catch (Exception e)
            {
                worker.ReportProgress((int)BwReturnProgressCode.IisOperationError, e.Message);
                return false;
            }

            return true;
        }

        private static bool CopyWebComponents(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopied);

            var fullWebApiPath = Path.Combine(currentInstallation.InstallationFolder, WebApiSubdir);
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathWebApi,
                fullWebApiPath, worker))
                return false;

            var fullWebClientPath = Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir);
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathWebClient,
                fullWebClientPath, worker))
                return false;


            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);
            return true;
        }

    }
}