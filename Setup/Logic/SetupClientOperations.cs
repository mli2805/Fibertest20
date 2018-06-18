using System.Collections.ObjectModel;

namespace Setup
{
    public class SetupClientOperations
    {
        private const string SourcePathClient = @"..\ClientFiles";
        private const string TargetPathClient = @"C:\IIT-Fibertest\Client\bin";

        public bool SetupClient(ObservableCollection<string> progressLines)
        {
            progressLines.Add("Client setup started.");

            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathClient, TargetPathClient, progressLines))
                return false;

            progressLines.Add("Shortcuts are created...");
            FileOperations.CreateShortcuts(TargetPathClient);
            progressLines.Add("Shortcuts are created successfully.");

            progressLines.Add("Client setup completed successfully.");
            return true;
        }
    }
}