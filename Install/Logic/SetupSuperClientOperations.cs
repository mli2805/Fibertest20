using System.ComponentModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
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
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathSuperClient, 
                fullSuperClientPath, worker))
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