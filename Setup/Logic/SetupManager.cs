using System.Collections.ObjectModel;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Setup
{
    public class SetupManager
    {
       
        private readonly CurrentInstallation _currentInstallation;
        private readonly IMyLog _logFile;
        private readonly SetupClientOperations _setupClientOperations;
        private readonly SetupDataCenterOperations _setupDataCenterOperations;
        private readonly SetupRtuManagerOperations _setupRtuManagerOperations;
        private readonly SetupUninstallOperations _setupUninstallOperations;

        public SetupManager(CurrentInstallation currentInstallation, IMyLog logFile,
            SetupClientOperations setupClientOperations,
            SetupDataCenterOperations setupDataCenterOperations, 
            SetupRtuManagerOperations setupRtuManagerOperations,
            SetupUninstallOperations setupUninstallOperations)
        {
            _currentInstallation = currentInstallation;
            _logFile = logFile;
            _setupClientOperations = setupClientOperations;
            _setupDataCenterOperations = setupDataCenterOperations;
            _setupRtuManagerOperations = setupRtuManagerOperations;
            _setupUninstallOperations = setupUninstallOperations;
        }

        public int Run(ObservableCollection<string> progressLines)
        {
            _logFile.AppendLine("Setup process started...");
            var count = 0;
            switch (_currentInstallation.InstallationType)
            {
                case InstallationType.Client:
                    if (!_setupClientOperations.SetupClient(progressLines, _currentInstallation.InstallationFolder))
                        return -1;
                    break;
                case InstallationType.Datacenter:
                    if (!_setupDataCenterOperations.SetupDataCenter(progressLines, _currentInstallation.InstallationFolder))
                        return -1;
                    break;
                case InstallationType.RtuManager:
                    count = _setupRtuManagerOperations.SetupRtuManager(progressLines, _currentInstallation.InstallationFolder);
                    if (count == -1)    return -1;
                    break;
            }

            _logFile.AppendLine("Setup uninstall application");
            _setupUninstallOperations.SetupUninstall(progressLines, _currentInstallation.InstallationFolder);
            return count;
        }
    }
}
