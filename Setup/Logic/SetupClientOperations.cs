using System.Collections.ObjectModel;
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

        public SetupClientOperations(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public bool SetupClient(BackgroundWorker worker, string installationFolder)
        {
            var fullClientPath = Path.Combine(installationFolder, ClientSubdir);
            _logFile.AppendLine($" full client path = {fullClientPath}");
            worker.ReportProgress(0, "Client setup started.");

            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathClient, fullClientPath, worker))
                return false;

            _logFile.AppendLine("Files are copied successfully");

            worker.ReportProgress(0, "Shortcuts are created...");
            ShortcutOperatios.CreateClientShortcut(fullClientPath);
            worker.ReportProgress(0, "Shortcuts are created successfully.");

            _logFile.AppendLine("Shortcuts are created successfully.");

            worker.ReportProgress(0, "Client setup completed successfully.");
            return true;
        }
    }
}