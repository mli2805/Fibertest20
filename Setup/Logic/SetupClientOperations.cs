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

            if (!SetupOperations.DirectoryCopyWithDecorations(SourcePathClient, TargetPathClient, progressLines))
                return;

            progressLines.Add("Shortcuts are created...");
            SetupOperations.CreateShortcuts(TargetPathClient);
            progressLines.Add("Shortcuts are created successfully.");
        }
    }
}