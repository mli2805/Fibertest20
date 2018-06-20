using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace Uninstall
{
    public class UnInstallFolderViewModel : PropertyChangedBase
    {
        private Visibility _visibility = Visibility.Collapsed;

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }


        public string MainName = "IIT Fibertest 2.0";

        public string Text1 { get; set; }
        public string InstallationFolder { get; set; }

        public UnInstallFolderViewModel()
        {
            Text1 = $"{MainName} will be uninstalled from the following folder. Click Uninstall to start the uninstallation.";
            InstallationFolder = RegistryOperations.GetFibertestValue("InstallationFolder", @"C:\IIT-Fibertest\");
        }
    }
}
