using System.Collections.ObjectModel;

namespace Setup
{
    public class SetupRtuManagerOperations
    {
        public void SetupRtuManager(ObservableCollection<string> progressLines)
        {
            progressLines.Add("RTU Manager setup started.");
        }
    }
}