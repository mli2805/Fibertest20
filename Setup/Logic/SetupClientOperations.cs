using System.Collections.ObjectModel;
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

        public bool SetupClient(ObservableCollection<string> progressLines, string installationFolder)
        {
            var fullClientPath = Path.Combine(installationFolder, ClientSubdir);
            _logFile.AppendLine($" full client path = {fullClientPath}");
            progressLines.Add("Client setup started.");

            if (FileOperations.DirectoryCopyWithDecorations(SourcePathClient, fullClientPath, progressLines) == -1)
                return false;

            _logFile.AppendLine("Files are copied successfully");

            progressLines.Add("Shortcuts are created...");
            ShortcutOperatios.CreateClientShortcut(fullClientPath);
            progressLines.Add("Shortcuts are created successfully.");

            _logFile.AppendLine("Shortcuts are created successfully.");

            progressLines.Add("Client setup completed successfully.");
            return true;
        }
    }
}