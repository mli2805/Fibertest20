using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class SetupDataCenterOperations
    {
        private readonly IMyLog _logFile;
        private const string WebApiSiteName = "fibertest_web_api";
        private const string WebClientSiteName = "fibertest_web_client";

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
            worker.ReportProgress((int)BwReturnProgressCode.DataCenterSetupStarted);
            if (!ServiceOperations.UninstallAllServicesOnThisPc(worker))
                return false;

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreBeingCopied);
            foreach (var service in FtServices.List.Where(s=>s.DestinationComputer == DestinationComputer.DataCenter))
            {
                if (!FileOperations.DirectoryCopyWithDecorations(service.SourcePath,
                    service.GetFullBinariesFolder(currentInstallation.InstallationFolder), worker))
                    return false;
            }
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);

            IniOperations.SaveMysqlTcpPort(currentInstallation.InstallationFolder,
                currentInstallation.MySqlTcpPort, currentInstallation.IsWebByHttps ? "https" : "http");

            var webApiService = FtServices.List.First(s => s.Name == "FibertestWebApiService");
            var settingsFilename = webApiService.FolderInsideFibertest + @"/ini/settings.json";
            File.WriteAllText(settingsFilename, currentInstallation.GetApiSettingsJson());
            
            if (!ServiceOperations.InstallSericesOnPc(DestinationComputer.DataCenter,
                currentInstallation.InstallationFolder, worker)) return false;

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

//            IisOperations.CreateWebsite(WebApiSiteName, bindingProtocol, $"*:{(int)TcpPorts.WebApiListenTo}:",
//                currentInstallation.SslCertificateName,
//                Path.Combine(currentInstallation.InstallationFolder, WebApiSubdir), worker);

            var webClientPort = currentInstallation.IsWebByHttps ? "*:443:" : "*:80:";
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
                SiteOperations.DeleteAllFibertestSitesOnThisPc(worker);
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
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreBeingCopied);

//            var fullWebApiPath = Path.Combine(currentInstallation.InstallationFolder, WebApiSubdir);
//            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathWebApi,
//                fullWebApiPath, worker))
//                return false;

            var fullWebClientPath = Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir);
            if (!FileOperations.DirectoryRemove(fullWebClientPath, worker))
                return false;
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathWebClient,
                fullWebClientPath, worker))
                return false;

            var settingsFilename = fullWebClientPath + @"/assets/settings.json";
            File.WriteAllText(settingsFilename, currentInstallation.GetApiSettingsJson());

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);
            return true;
        }

    }

    
}