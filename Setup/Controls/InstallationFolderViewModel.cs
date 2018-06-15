using System.IO;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;

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
        private string _destinationFolder;

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

        public string DestinationFolder
        {
            get { return _destinationFolder; }
            set
            {
                if (value == _destinationFolder) return;
                _destinationFolder = value;
                NotifyOfPropertyChange();
            }
        }

        public InstallationFolderViewModel(CurrentInstallation currentInstallation)
        {
            HeaderViewModel.InBold = "Choose Install Location";
            HeaderViewModel.Explanation = $"Choose folder in which to install {currentInstallation.MainName}";
            Text1 =
                $"Setup will install {currentInstallation.MainName} in the following folder. To install in a different folder, click Browse and select another folder. Click Next to continue.";
            DestinationFolder = @"C:\IIT-Fibertest\";
            SpaceAvailable = $"Space available: {SpaceToString(GetAvailableFreeSpace(@"C:\"))}";
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
            using (var dialog = new FolderBrowserDialog(){SelectedPath = @"C:\IIT-Fibertest\", ShowNewFolderButton = true})
            {
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                    DestinationFolder = dialog.SelectedPath;
            }
        }
    }
}
