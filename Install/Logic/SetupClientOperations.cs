using System.ComponentModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class SetupClientOperations
    {
        private readonly IMyLog _logFile;
        private const string SourcePathClient = @"..\ClientFiles";
        private const string ClientSubdir = @"Client\bin";
        private const string SourcePathReflect = @"..\RftsReflect";
        private const string ReflectSubdir = @"RftsReflect";
        private const string SourcePathUserGuide = @"..\UserGuide";
        private const string UserGuideSubdir = @"..\UserGuide";


        public SetupClientOperations(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public bool SetupClient(BackgroundWorker worker, string installationFolder)
        {
            worker.ReportProgress((int)BwReturnProgressCode.ClientSetupStarted);
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreBeingCopied);

            var fullClientPath = Path.Combine(installationFolder, ClientSubdir);
            _logFile.AppendLine($" full client path = {fullClientPath}");
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathClient,
                fullClientPath, worker))
                return false;

            var userGuideFolder = Path.Combine(fullClientPath, UserGuideSubdir);
            _logFile.AppendLine($" full userGuide path = {userGuideFolder}");
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathUserGuide,
                userGuideFolder, worker))
                return false;


            var fullReflectPath = Path.Combine(installationFolder, ReflectSubdir);
            _logFile.AppendLine($" full Reflect path = {fullReflectPath}");
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathReflect,
                fullReflectPath, worker))
                return false;
            if (!FileOperations.CleanAntiGhost(fullReflectPath, false, worker))
                return false;

            if (!Directory.Exists(fullReflectPath + "\\Share"))
                Directory.CreateDirectory(fullReflectPath + "\\Share");

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);
            _logFile.AppendLine("Files are copied successfully");

            ShortcutOperatios.CreateClientShortcut(fullClientPath);
            ShortcutOperatios.CreateReflectShortcut(fullReflectPath);
            worker.ReportProgress((int)BwReturnProgressCode.ShortcutsAreCreatedSuccessfully);

            _logFile.AppendLine("Shortcuts are created successfully.");

            worker.ReportProgress((int)BwReturnProgressCode.ClientSetupCompletedSuccessfully);
            return true;
        }
    }
}