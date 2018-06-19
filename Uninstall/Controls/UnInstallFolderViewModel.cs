using Caliburn.Micro;

namespace Uninstall
{
    public class UnInstallFolderViewModel : PropertyChangedBase
    {
        public string MainName = "IIT Fibertest 2.0";

        public string Text1 { get; set; }

        public UnInstallFolderViewModel()
        {
            Text1 = $"{MainName} will be uninstalled from the following folder. Click Uninstall to start the uninstallation.";

        }
    }
}
