using System.Collections.ObjectModel;
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

        public void SetupUninstall(ObservableCollection<string> progressLines, string installationFolder)
        {
            progressLines.Add("Uninstall setup started.");

            var fullUninstallPath = Path.Combine(installationFolder, UninstallSubdir);
            _logFile.AppendLine($" full uninstall path = {fullUninstallPath}");
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathUninstall, fullUninstallPath, progressLines))
                return;

            _logFile.AppendLine("Files are copied successfully");

            progressLines.Add("Shortcuts are created...");
            ShortcutOperatios.CreateUninstallShortcut(fullUninstallPath);
            progressLines.Add("Shortcuts are created successfully.");

            progressLines.Add("Uninstall setup completed successfully.");
        }

    }
}