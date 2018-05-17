using System.Diagnostics;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class AboutViewModel : Screen
    {
        public string ServerVersion { get; set; }
        public string ClientVersion { get; set; }

        public AboutViewModel(CurrentDatacenterParameters currentDatacenterParameters)
        {
            ServerVersion = currentDatacenterParameters.DatacenterVersion;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            ClientVersion = fvi.FileVersion;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_About;
        }

        public void Close() { TryClose(); }
    }
}
