using System.ComponentModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Setup
{
    public class SetupUninstallOperations
    {
        private readonly IMyLog _logFile;
        private const string SourcePathUninstall = @"..\UninstallFiles";
        private const string UninstallSubdir = @"Uninstall";

        public SetupUninstallOperations(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void SetupUninstall(BackgroundWorker worker, string installationFolder)
        {
            worker.ReportProgress((int)BwReturnProgressCode.UninstallSetupStarted);

            var fullUninstallPath = Path.Combine(installationFolder, UninstallSubdir);
            _logFile.AppendLine($" full uninstall path = {fullUninstallPath}");
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathUninstall, fullUninstallPath, worker))
                return;

            _logFile.AppendLine("Files are copied successfully");

            ShortcutOperatios.CreateUninstallShortcut(fullUninstallPath);
            worker.ReportProgress((int)BwReturnProgressCode.UninstallSetupCompletedSuccessfully);
        }

    }
}