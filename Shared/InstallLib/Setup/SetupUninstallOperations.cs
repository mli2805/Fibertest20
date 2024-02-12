using System.ComponentModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.InstallLib
{
    public class SetupUninstallOperations
    {
        private readonly IMyLog _logFile;
        private const string SourcePathUninstall = @"..\UninstallFiles";
        private const string UninstallSubdir = @"Uninstall\bin";

        public SetupUninstallOperations(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public bool SetupUninstall(BackgroundWorker worker, string installationFolder, bool isOnRtu)
        {
            worker.ReportProgress((int)BwReturnProgressCode.UninstallSetupStarted);

            var fullUninstallPath = Path.Combine(installationFolder, UninstallSubdir);
            _logFile.AppendLine($" full uninstall path = {fullUninstallPath}");
            if (!InstallFileOperations.DirectoryCopyWithDecorations(SourcePathUninstall, 
                fullUninstallPath, worker))
                return false;

            var iniUninstallPath = Path.Combine(installationFolder, @"Uninstall\ini");

            var iniFile = new IniFile();
            var iniFileName = Utils.FileNameForSure(iniUninstallPath, "Uninstall.ini",
                false, true);
            iniFile.AssignFile(iniFileName, true);
            iniFile.Write(IniSection.Uninstall, IniKey.IsOnRtu, isOnRtu);

            _logFile.AppendLine("Files are copied successfully");

            ShortcutOperatios.CreateUninstallShortcut(fullUninstallPath);
            worker.ReportProgress((int)BwReturnProgressCode.UninstallSetupCompletedSuccessfully);
            return true;
        }

    }
}