using System.ComponentModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Setup
{
    public class SetupClientOperations
    {
        private readonly IMyLog _logFile;
        private const string SourcePathClient = @"..\ClientFiles";
        private const string ClientSubdir = @"Client\bin";
        private const string SourcePathReflect = @"..\RftsReflect";
        private const string ReflectSubdir = @"RftsReflect";


        public SetupClientOperations(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public bool SetupClient(BackgroundWorker worker, string installationFolder)
        {
            worker.ReportProgress(0, "Client setup started.");
            worker.ReportProgress(0, "Files are copied...");

            var fullClientPath = Path.Combine(installationFolder, ClientSubdir);
            _logFile.AppendLine($" full client path = {fullClientPath}");
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathClient, fullClientPath, worker))
                return false;

            var fullReflectPath = Path.Combine(installationFolder, ReflectSubdir);
            _logFile.AppendLine($" full Reflect path = {fullReflectPath}");
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathReflect, fullReflectPath, worker))
                return false;
            
            worker.ReportProgress(1, "");
            _logFile.AppendLine("Files are copied successfully");

            worker.ReportProgress(0, "Shortcuts are created...");
            ShortcutOperatios.CreateClientShortcut(fullClientPath);
            ShortcutOperatios.CreateReflectShortcut(fullReflectPath);
            worker.ReportProgress(0, "Shortcuts are created successfully.");

            _logFile.AppendLine("Shortcuts are created successfully.");

            worker.ReportProgress(0, "Client setup completed successfully.");
            return true;
        }
    }
}