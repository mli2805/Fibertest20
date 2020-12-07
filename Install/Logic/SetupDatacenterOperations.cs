﻿using System;
using System.ComponentModel;
using System.IO;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class DeleteDataCenter
    {

    }
    public class SetupDataCenterOperations
    {
        private readonly IMyLog _logFile;
        private const string DataCenterServiceName = "FibertestDcService";
        private const string DataCenterWebApiServiceName = "FibertestWaService";

        private const string WebApiSiteName = "fibertest_web_api";
        private const string WebClientSiteName = "fibertest_web_client";

        private const string SourcePathDataCenter = @"..\DcFiles";
        private const string DataCenterSubdir = @"DataCenter\bin";
        private const string ServiceFilename = @"Iit.Fibertest.DataCenterService.exe";

        private const string SourcePathWebApi = @"..\WebApi";
        private const string WebApiSubdir = @"WebApi\publish";
        private const string SourcePathWebClient = @"..\WebClient";
        private const string WebClientSubdir = @"WebClient";

        private const string SourcePathUserGuide = @"..\UserGuide";
        private const string UserGuideSubdir = @"assets\UserGuide";

        public SetupDataCenterOperations(IMyLog logFile)
        {
            _logFile = logFile;
        }

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
            if (!ServiceOperations.UninstallServiceIfExist(DataCenterServiceName, worker))
                return false;
            if (!ServiceOperations.UninstallServiceIfExist(DataCenterWebApiServiceName, worker))
                return false;

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopied);
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathDataCenter,
                fullDataCenterPath, worker))
                return false;
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);

            IniOperations.SaveMysqlTcpPort(currentInstallation.InstallationFolder,
                currentInstallation.MySqlTcpPort, currentInstallation.IsWebByHttps ? "https" : "http");

            var filename = Path.Combine(fullDataCenterPath, ServiceFilename);
            if (!ServiceOperations.InstallService(DataCenterServiceName,filename, worker)) return false;

            worker.ReportProgress((int)BwReturnProgressCode.DataCenterSetupCompletedSuccessfully);
            return true;
        }

        private bool SetupWebComponents(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.WebComponentsSetupStarted);

            if (!DeleteExistingWebSites(worker, currentInstallation))
                if (!DeleteExistingWebSites(worker, currentInstallation)) // second attempt
                    return false;

            if (!CopyWebComponents(worker, currentInstallation))
                return false;

            if (!currentInstallation.IsWebByHttps)
                currentInstallation.SslCertificateName = null;

            var bindingProtocol = currentInstallation.IsWebByHttps ? "https" : "http";
            var webClientPort = currentInstallation.IsWebByHttps ? "*:443:" : "*:80:";

            IisOperations.CreateWebsite(WebApiSiteName, bindingProtocol, $"*:{(int)TcpPorts.WebApiListenTo}:",
                currentInstallation.SslCertificateName,
                Path.Combine(currentInstallation.InstallationFolder, WebApiSubdir), worker);

            var fullWebClientPath = Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir);
            IisOperations.CreateWebsite(WebClientSiteName, bindingProtocol, webClientPort,
                currentInstallation.SslCertificateName, fullWebClientPath, worker);

            var userGuideFolder = Path.Combine(fullWebClientPath, UserGuideSubdir);
            _logFile.AppendLine($" full userGuide path = {userGuideFolder}");
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathUserGuide,
                userGuideFolder, worker))
                return false;

            worker.ReportProgress((int)BwReturnProgressCode.WebComponentsSetupCompletedSuccessfully);
            return true;
        }

        private static bool DeleteExistingWebSites(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            try
            {
                if (!DeleteOneWebSite(worker, currentInstallation, WebApiSiteName))
                    if (!DeleteOneWebSite(worker, currentInstallation, WebApiSiteName)) // second attempt
                        return false;
                if (!DeleteOneWebSite(worker, currentInstallation, WebClientSiteName))
                    if (!DeleteOneWebSite(worker, currentInstallation, WebClientSiteName)) // second attempt
                        return false;
                return true;
            }
            catch (Exception e)
            {
                worker.ReportProgress((int)BwReturnProgressCode.IisOperationError, e.Message);
                return false;
            }
        }

        private static bool DeleteOneWebSite(BackgroundWorker worker, CurrentInstallation currentInstallation, string siteName)
        {
            try
            {
                if (IisOperations.DoesWebsiteExist(siteName))
                    WebCommonOperation.DeleteWebsite(siteName, worker);

                var webSitePath = Path.Combine(currentInstallation.InstallationFolder, siteName);
                if (Directory.Exists(webSitePath))
                    Directory.Delete(webSitePath, true);
                return true;
            }
            catch (Exception e)
            {
                worker.ReportProgress((int)BwReturnProgressCode.IisOperationError, e.Message);
                return false;
            }
        }

        private static bool CopyWebComponents(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopied);

            var fullWebApiPath = Path.Combine(currentInstallation.InstallationFolder, WebApiSubdir);
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathWebApi,
                fullWebApiPath, worker))
                return false;

            var fullWebClientPath = Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir);
            if (!FileOperations.DirectoryRemove(fullWebClientPath, worker))
                return false;
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathWebClient,
                fullWebClientPath, worker))
                return false;

            var settingsFilename = fullWebClientPath + @"/assets/settings.json";
            var webClientSettings = new WebClientSettings()
            {
                ApiProtocol = currentInstallation.IsWebByHttps
                ? "https"
                : "http",
                ApiPort = (int)TcpPorts.WebApiListenTo,
                Version = currentInstallation.ProductVersion
            };
            var json = webClientSettings.ToCamelCaseJson();

            File.WriteAllText(settingsFilename, json);

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);
            return true;
        }

    }

    public class WebClientSettings
    {
        public string ApiProtocol;
        public int ApiPort;
        public string Version;
    }
}