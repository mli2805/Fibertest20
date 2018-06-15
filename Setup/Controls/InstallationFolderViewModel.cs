using System.IO;
using System.Windows;
using Caliburn.Micro;

namespace Setup
{
    public class InstallationFolderViewModel : Screen
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

        public string DestinationFolder { get; set; }

        public InstallationFolderViewModel(CurrentInstallation currentInstallation)
        {
            HeaderViewModel.InBold = "Choose Install Location";
            HeaderViewModel.Explanation = $"Choose folder in which to install {currentInstallation.MainName}";
            Text1 =
                $"Setup will install {currentInstallation.MainName} in the following folder. To install in a different folder, click Browse and select another folder. Click Next to continue.";
            DestinationFolder = @"C:\IIT-Fibertest\";
            SpaceAvailable = $"Space available: {((int) (GetAvailableFreeSpace(@"C:\") / 1024 / 1024)):##,###} MB";
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
