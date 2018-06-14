using System.IO;
using System.Windows;
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

        private string _spaceAvailable;
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

        public string DefaultFibertest20Folder = @"C:\IIT-Fibertest20\";

        public InstallationFolderViewModel()
        {
            HeaderViewModel.InBold = "Choose Install Location";
            HeaderViewModel.Explanation = "Choose folder in which to install IIT Fibertest 2.0";

            SpaceAvailable = $"Space available: {(int) (GetAvailableFreeSpace(@"C:\") / 1024 / 1024)} Mb";
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
    }
}
