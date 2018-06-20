using System.Collections.ObjectModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Setup
{
    public class SetupClientOperations
    {
        private const string SourcePathClient = @"..\ClientFiles";
        private const string ClientSubdir = @"Client\bin";

        public bool SetupClient(ObservableCollection<string> progressLines, string installationFolder)
        {
            var fullClientPath = Path.Combine(installationFolder, ClientSubdir);

            progressLines.Add("Client setup started.");

            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathClient, fullClientPath, progressLines))
                return false;

            progressLines.Add("Shortcuts are created...");
            ShortcutOperatios.CreateClientShortcut(fullClientPath);
            progressLines.Add("Shortcuts are created successfully.");

            progressLines.Add("Client setup completed successfully.");
            return true;
        }
    }
}