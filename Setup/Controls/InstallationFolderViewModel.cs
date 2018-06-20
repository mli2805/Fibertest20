using System.IO;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Setup
{
    public class InstallationFolderViewModel : PropertyChangedBase
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
        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();

        public string Text1 { get; set; }
        private string _spaceAvailable;
        private string _installationFolder;

        public string SpaceAvailable
        {
            get => _spaceAvailable;
            set
            {
                if (value == _spaceAvailable) return;
                _spaceAvailable = value;
                NotifyOfPropertyChange();
            }
        }

        public string InstallationFolder
        {
            get { return _installationFolder; }
            set
            {
                if (value == _installationFolder) return;
                _installationFolder = value;
                NotifyOfPropertyChange();
            }
        }

        public InstallationFolderViewModel(CurrentInstallation currentInstallation)
        {
            HeaderViewModel.InBold = Resources.SID_Choose_Install_Location;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Choose_folder_in_which_to_install__0_, currentInstallation.MainName);
            Text1 = string.Format(Resources.SID_Setup_will_install__0__in_, currentInstallation.MainName);
            InstallationFolder = RegistryOperations.GetFibertestValue("InstallationFolder", @"C:\IIT-Fibertest\");
            SpaceAvailable = string.Format(Resources.SID_Space_available___0_, SpaceToString(GetAvailableFreeSpace(@"C:\")));
        }

        private long GetAvailableFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.AvailableFreeSpace;
                }
            }
            return -1;
        }

        private string SpaceToString(double number)
        {
            return number < 1024
                ? $"{number} B"
                : number / 1024 < 1024
                    ? $"{number / 1024:##,###.#} KB"
                    : number / 1024 / 1024 < 1024
                        ? $"{number / 1024 / 1024:##,###.#} MB"
                        : number / 1024 / 1024 / 1024 < 1024
                            ? $"{number / 1024 / 1024 / 1024:##,###.#} GB"
                            : $"{number / 1024 / 1024 / 1024 / 1024:##,###.#} TB";
        }

        public void Browse()
        {
            using (var dialog = new FolderBrowserDialog(){SelectedPath = InstallationFolder, ShowNewFolderButton = true})
            {
                var result = dialog.ShowDialog();
                if (result != DialogResult.OK) return;

                InstallationFolder = dialog.SelectedPath;
            }
        }
    }
}
