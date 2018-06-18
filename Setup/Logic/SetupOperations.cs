using System.Collections.ObjectModel;
using System.IO;
using IWshRuntimeLibrary;

namespace Setup
{
    public class SetupOperations
    {
        private const string DataCenterServiceName = "FibertestDcService";
        private const string DataCenterDisplayName = "Fibertest 2.0 DataCenter Server";
        private const string SourcePathClient = @"..\ClientFiles";
        private const string TargetPathBase = @"C:\IIT-Fibertest\";
        private readonly string _targetPathClient;

        private readonly CurrentInstallation _currentInstallation;
        private ObservableCollection<string> _progressLines;

        public SetupOperations(CurrentInstallation currentInstallation)
        {
            _currentInstallation = currentInstallation;
            _targetPathClient = TargetPathBase + @"Client\bin";
        }

        public void Run(ObservableCollection<string> progressLines)
        {
            _progressLines = progressLines;

            switch (_currentInstallation.InstallationType)
            {
                case InstallationType.Client:
                    SetupClient();
                    break;
                case InstallationType.Datacenter:
                    SetupDataCenter();
                    break;
                case InstallationType.RtuManager:
                    SetupRtuManager();
                    break;

            }
        }

        private void SetupClient()
        {
            _progressLines.Add("Client setup started.");

            _progressLines.Add("Files are copied...");
            if (DirectoryCopy(SourcePathClient, _targetPathClient))
                _progressLines.Add("Files are copied successfully.");

            _progressLines.Add("Shortcuts are created...");
            CreateShortcuts();
            _progressLines.Add("Shortcuts are created successfully.");
        }

        private void CreateShortcuts()
        {
            object shDesktop = "Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string) shell.SpecialFolders.Item(ref shDesktop) + @"\FtClient20.lnk";
            IWshShortcut shortcut = (IWshShortcut) shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Fibertest 2.0 Client";
            shortcut.TargetPath = _targetPathClient + @"\Iit.Fibertest.Client.exe";
            shortcut.IconLocation = _targetPathClient + @"\Iit.Fibertest.Client.exe";
            shortcut.Save();
        }

        private bool DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                _progressLines.Add("Error! Source folder not found!");
                return false;
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                _progressLines.Add(temppath);
                file.CopyTo(temppath, true);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
            }

            return true;
        }

        private void SetupDataCenter()
        {
            _progressLines.Add("Data Center setup started.");

            UninstallDcServiceIfNeeded();
        }

        private void SetupRtuManager()
        {
            _progressLines.Add("RTU Manager setup started.");
        }

        private void UninstallDcServiceIfNeeded()
        {
            if (!ServiceInstaller.ServiceIsInstalled(DataCenterServiceName)) return;

            if (!StopServiceIfRunning()) return;

            _progressLines.Add("Data Center service uninstalling...");
            ServiceInstaller.Uninstall(DataCenterServiceName);
            if (ServiceInstaller.ServiceIsInstalled(DataCenterServiceName))
            {
                _progressLines.Add($"Cannot uninstall service {DataCenterDisplayName}");
                return;
            }

            _progressLines.Add($"Service {DataCenterDisplayName} uninstalled successfully.");
        }

        private bool StopServiceIfRunning()
        {
            if (ServiceInstaller.GetServiceStatus(DataCenterServiceName) != ServiceState.Running) return true;

            _progressLines.Add("Data Center service stopping...");
            ServiceInstaller.StopService(DataCenterServiceName);
            if (ServiceInstaller.GetServiceStatus(DataCenterServiceName) != ServiceState.Stopped)
            {
                _progressLines.Add($"Cannot stop service {DataCenterDisplayName}");
                return false;
            }

            _progressLines.Add($"Service {DataCenterDisplayName} stopped succesfully.");
            return true;
        }
    }
}
