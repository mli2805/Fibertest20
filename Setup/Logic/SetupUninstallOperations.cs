using System.Collections.ObjectModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Setup
{
    public class SetupUninstallOperations
    {
        private const string SourcePathUninstall = @"..\UninstallFiles";
        private const string UninstallSubdir = @"Uninstall";

        public void SetupUninstall(ObservableCollection<string> progressLines, string installationFolder)
        {
            progressLines.Add("Uninstall setup started.");

            var fullUninstallPath = Path.Combine(installationFolder, UninstallSubdir);
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathUninstall, fullUninstallPath, progressLines))
                return;

            progressLines.Add("Shortcuts are created...");
            FileOperations.CreateUninstallShortcut(fullUninstallPath);
            progressLines.Add("Shortcuts are created successfully.");

            progressLines.Add("Uninstall setup completed successfully.");
        }

    }
}