using System.ComponentModel;
using System.IO;
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

        public bool SetupDataCenter(BackgroundWorker worker, string installationFolder, string mysqlTcpPort)
        {
            var fullDataCenterPath = Path.Combine(installationFolder, DataCenterSubdir);

            worker.ReportProgress((int)BwReturnProgressCode.DataCenterSetupStarted);
            if (!ServiceOperations.UninstallServiceIfExist(DataCenterServiceName, DataCenterDisplayName, worker))
                return false;

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopied);
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathDataCenter, fullDataCenterPath, worker))
                return false;

            SaveMysqlTcpPort(installationFolder, mysqlTcpPort);

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);

            var filename = Path.Combine(fullDataCenterPath, ServiceFilename);
            if (!ServiceOperations.InstallService(DataCenterServiceName, 
                DataCenterDisplayName, DataCenterServiceDescription, filename, worker)) return false;

            worker.ReportProgress((int)BwReturnProgressCode.DataCenterSetupCompletedSuccessfully);
            return true;
        }

        private static void SaveMysqlTcpPort(string installationFolder, string mysqlTcpPort)
        {
            var iniDataCenterPath = Path.Combine(installationFolder, DataCenterIniSubdir);
            var iniFileName = Path.Combine(iniDataCenterPath, "DataCenter.ini");
            
            var iniFile = new IniFile();
            iniFile.AssignFile(iniFileName);

            iniFile.Write(IniSection.MySql, IniKey.MySqlTcpPort, mysqlTcpPort);
        }
    }

    public class SetupSuperClientOperations
    {
        private readonly IMyLog _logFile;
        private const string SourcePathSuperClient = @"..\SuperClientFiles";
        private const string SuperClientSubdir = @"SuperClient\bin";

        public SetupSuperClientOperations(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public bool SetupSuperClient(BackgroundWorker worker, string installationFolder)
        {
            worker.ReportProgress((int)BwReturnProgressCode.SuperClientSetupStarted);
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopied);

            var fullSuperClientPath = Path.Combine(installationFolder, SuperClientSubdir);
            _logFile.AppendLine($" full super-client path = {fullSuperClientPath}");
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathSuperClient, fullSuperClientPath, worker))
                return false;

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);
            _logFile.AppendLine("Files are copied successfully");

            ShortcutOperatios.CreateSuperClientShortcut(fullSuperClientPath);
            worker.ReportProgress((int)BwReturnProgressCode.ShortcutsAreCreatedSuccessfully);
            _logFile.AppendLine("Shortcuts are created successfully.");

            worker.ReportProgress((int)BwReturnProgressCode.SuperClientSetupCompletedSuccessfully);
            return true;
        }
    }
}