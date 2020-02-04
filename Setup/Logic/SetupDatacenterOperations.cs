using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Setup
{
    public class SetupDataCenterOperations
    {
        private const string DataCenterServiceName = "FibertestDcService";
        private const string DataCenterDisplayName = "Fibertest 2.0 DataCenter Server";
        private const string DataCenterServiceDescription = "Fibertest 2.0 DataCenter Server Service";

        private const string SourcePathDataCenter = @"..\DcFiles";
        private const string DataCenterSubdir = @"DataCenter\bin";
        private const string DataCenterIniSubdir = @"DataCenter\ini";
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

            SaveMysqlTcpPort(currentInstallation.InstallationFolder, currentInstallation.MySqlTcpPort);


            var filename = Path.Combine(fullDataCenterPath, ServiceFilename);
            if (!ServiceOperations.InstallService(DataCenterServiceName,
                DataCenterDisplayName, DataCenterServiceDescription, filename, worker)) return false;

            worker.ReportProgress((int)BwReturnProgressCode.DataCenterSetupCompletedSuccessfully);
            return true;
        }

        private static bool SetupWebComponents(BackgroundWorker worker, CurrentInstallation currentInstallation)
        {
            worker.ReportProgress((int)BwReturnProgressCode.WebComponentsSetupStarted);
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreUnziped);

            var extractingPath = Path.GetDirectoryName(currentInstallation.WebArchivePath) + @"\ExtractedWebFiles";
            ZipFile.ExtractToDirectory(currentInstallation.WebArchivePath, extractingPath);

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

        private static void SaveMysqlTcpPort(string installationFolder, string mysqlTcpPort)
        {
            var iniDataCenterPath = Path.Combine(installationFolder, DataCenterIniSubdir);

            var iniFile = new IniFile();
            var iniFileName = Utils.FileNameForSure(iniDataCenterPath, "DataCenter.ini",
                false, true);
            iniFile.AssignFile(iniFileName, true);

            iniFile.Write(IniSection.MySql, IniKey.MySqlTcpPort, mysqlTcpPort);
        }
    }
}