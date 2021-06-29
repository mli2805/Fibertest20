using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Uninstall
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
        public bool IsFullUninstall { get; set; }

        public string CheckContent { get; set; }
        public Visibility FullUninstallVisibility { get; set; }

        public UnInstallFolderViewModel(Visibility fullUninstallVisibility)
        {
            FullUninstallVisibility = fullUninstallVisibility;
            Text1 = string.Format(Resources.SID__0__will_be_uninstalled_from_the_following_folder__Click_Uninstall_to_start_the_uninstallation_, MainName);
            InstallationFolder = RegistryOperations.GetFibertestValue("InstallationFolder", "");
            if (string.IsNullOrEmpty(InstallationFolder))
                InstallationFolder = @"C:\IIT-Fibertest\";
            CheckContent = Resources.SID_Full_uninstall__Will_be_deleted_all_user_data___ini__log__map__etc__;
        }
    }
}
