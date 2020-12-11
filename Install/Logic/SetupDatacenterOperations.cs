using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class SetupDataCenterOperations
    {
        private const string WebApiSiteName = "fibertest_web_api";
        private const string WebClientSiteName = "fibertest_web_client";

        private const string SourcePathWebClient = @"..\WebClient";
        private const string WebClientSubdir = @"WebClient";

        private const string SourcePathUserGuide = @"..\UserGuide";
        private const string UserGuideSubdir = @"assets\UserGuide";

        public bool SetupDataCenter(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.DataCenterSetupStarted);

            if (!FtServices.List
                .Where(s => s.DestinationComputer == DestinationComputer.DataCenter)
                .All(service => ServiceOperations.UninstallServiceIfExist(service, worker)))
                return false;

            if (!DeleteExistingWebSites(worker, currentInstallation))
                if (!DeleteExistingWebSites(worker, currentInstallation)) // second attempt
                    return false;

            if (!SetupDataCenterComponent(worker, currentInstallation)) return false;

            if (currentInstallation.IsWebNeeded)
            {
                if (!SetupWebApiComponent(worker, currentInstallation)) return false;
                if (!SetupWebClientComponent(worker, currentInstallation)) return false;
            }

            worker.ReportProgress((int)BwReturnProgressCode.DataCenterSetupCompletedSuccessfully);
            return true;
        }

        private static bool SetupDataCenterComponent(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreBeingCopied);
            var service = FtServices.List.First(s => s.Name == "FibertestDcService");
            if (!FileOperations.DirectoryCopyWithDecorations(service.SourcePath,
                service.GetFullBinariesFolder(currentInstallation.InstallationFolder), worker))
                return false;
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);

            PlaceDataCenterParamsIntoIniFile(currentInstallation);

            return ServiceOperations.InstallService(service, currentInstallation.InstallationFolder, worker);
        }

        private static void PlaceDataCenterParamsIntoIniFile(CurrentInstallation currentInstallation)
        {
            var webApiBindingProtocol = currentInstallation.IsWebNeeded
                ? currentInstallation.IsWebByHttps
                    ? "https"
                    : "http"
                : "none";
            IniOperations.PlaceParamsIntoIniFile(currentInstallation.InstallationFolder,
                currentInstallation.MySqlTcpPort, webApiBindingProtocol);
        }

        private static bool SetupWebApiComponent(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreBeingCopied);
            var service = FtServices.List.First(s => s.Name == "FibertestWaService");
            if (!FileOperations.DirectoryCopyWithDecorations(service.SourcePath,
                service.GetFullBinariesFolder(currentInstallation.InstallationFolder), worker))
                return false;
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);

            var settingsFilename = currentInstallation.InstallationFolder + service.FolderInsideFibertest + @"/ini/settings.json";
            File.WriteAllText(settingsFilename, currentInstallation.GetApiSettingsJson());

            return ServiceOperations.InstallService(service, currentInstallation.InstallationFolder, worker);
        }

        private bool SetupWebClientComponent(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.WebComponentsSetupStarted);

            if (!CopyWebClientComponent(worker, currentInstallation))
                return false;

            if (!currentInstallation.IsWebByHttps)
                currentInstallation.SslCertificateName = null;

            var bindingProtocol = currentInstallation.IsWebByHttps ? "https" : "http";
            var webClientPort = currentInstallation.IsWebByHttps ? "*:443:" : "*:80:";
            IisOperations.CreateWebsite(WebClientSiteName, bindingProtocol, webClientPort,
                currentInstallation.SslCertificateName, Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir), worker);
          
            worker.ReportProgress((int)BwReturnProgressCode.WebComponentsSetupCompletedSuccessfully);
            return true;
        }

        private static bool CopyWebClientComponent(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreBeingCopied);

            var fullWebClientPath = Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir);
            if (!FileOperations.DirectoryRemove(fullWebClientPath, worker))
                return false;
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathWebClient,
                fullWebClientPath, worker))
                return false;

            var settingsFilename = fullWebClientPath + @"/assets/settings.json";
            File.WriteAllText(settingsFilename, currentInstallation.GetApiSettingsJson());

            var userGuideFolder = Path.Combine(fullWebClientPath, UserGuideSubdir);
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathUserGuide,
                userGuideFolder, worker))
                return false;

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);
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
    }


}