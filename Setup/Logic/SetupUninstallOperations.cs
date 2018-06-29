using System.Collections.ObjectModel;
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
            worker.ReportProgress(0, "Uninstall setup started.");

            var fullUninstallPath = Path.Combine(installationFolder, UninstallSubdir);
            _logFile.AppendLine($" full uninstall path = {fullUninstallPath}");
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathUninstall, fullUninstallPath, worker))
                return;

            _logFile.AppendLine("Files are copied successfully");

            worker.ReportProgress(0, "Shortcuts are created...");
            ShortcutOperatios.CreateUninstallShortcut(fullUninstallPath, worker);
            worker.ReportProgress(0, "Shortcuts are created successfully.");

            worker.ReportProgress(0, "Uninstall setup completed successfully.");
        }

    }
}