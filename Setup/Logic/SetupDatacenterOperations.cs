using System;
using System.ComponentModel;
using System.IO;
using Iit.Fibertest.UtilsLib;
using Ionic.Zip;

namespace Iit.Fibertest.Setup
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

        private const string SourcePathWebApi = @"\WebApi";
        private const string WebApiSubdir = @"WebApi\publish";
        private const string SourcePathWebClient = @"\WebClient";
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
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathDataCenter, true,
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

            var extractingPath = UnzipWebComponents(worker, currentInstallation);

            if (!DeleteExistingWebSites(worker, currentInstallation))
                return false;

            if (!CopyWebComponents(worker, currentInstallation, extractingPath))
                return false;


            IisOperations.CreateWebsite(WebApiSiteName, "http", "*:11080:",
                Path.Combine(currentInstallation.InstallationFolder, WebApiSubdir), worker);

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
                    Directory.Delete(Path.Combine(currentInstallation.InstallationFolder, WebApiSubdir), true);
                }

                if (IisOperations.DoesWebsiteExist(WebClientSiteName))
                {
                    IisOperations.DeleteWebsite(WebClientSiteName, worker);
                    Directory.Delete(Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir), true);
                }
            }
            catch (Exception e)
            {
                worker.ReportProgress((int)BwReturnProgressCode.IisOperationError, e.Message);
                return false;
            }

            return true;
        }

        private static bool CopyWebComponents(BackgroundWorker worker, CurrentInstallation currentInstallation,
            string extractingPath)
        {
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopied);

            var fullWebApiSourcePath = extractingPath + SourcePathWebApi;
            var fullWebApiPath = Path.Combine(currentInstallation.InstallationFolder, WebApiSubdir);
            if (!FileOperations.DirectoryCopyWithDecorations(fullWebApiSourcePath, false,
                fullWebApiPath, worker))
                return false;

            var fullWebClientSourcePath = extractingPath + SourcePathWebClient;
            var fullWebClientPath = Path.Combine(currentInstallation.InstallationFolder, WebClientSubdir);
            if (!FileOperations.DirectoryCopyWithDecorations(fullWebClientSourcePath, false,
                fullWebClientPath, worker))
                return false;

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);
            return true;
        }

        private static string UnzipWebComponents(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreUnziped);

            var currentDomain = AppDomain.CurrentDomain.BaseDirectory;
            var extractingPath = currentDomain + @"ExtractedWebFiles";
            if (Directory.Exists(extractingPath))
                Directory.Delete(extractingPath, true);
            using (ZipFile zipFile = ZipFile.Read(currentInstallation.WebArchivePath))
            {
                foreach (var zipEntry in zipFile)
                {
                    zipEntry.ExtractWithPassword(extractingPath, "");
                }
            }

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreUnzipedSuccessfully);
            return extractingPath;
        }

    
    }
}