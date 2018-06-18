using System.Collections.ObjectModel;

namespace Setup
{
    public class SetupClientOperations
    {
        private const string SourcePathClient = @"..\ClientFiles";
        private const string TargetPathClient = @"C:\IIT-Fibertest\Client\bin";

        public void SetupClient(ObservableCollection<string> progressLines)
        {
            progressLines.Add("Client setup started.");

            progressLines.Add("Files are copied...");
            if (SetupOperations.DirectoryCopy(SourcePathClient, TargetPathClient, progressLines))
                progressLines.Add("Files are copied successfully.");

            progressLines.Add("Shortcuts are created...");
            SetupOperations.CreateShortcuts(TargetPathClient);
            progressLines.Add("Shortcuts are created successfully.");
        }
    }
}