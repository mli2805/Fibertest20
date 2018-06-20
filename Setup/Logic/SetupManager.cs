using System.Collections.ObjectModel;

namespace Iit.Fibertest.Setup
{
    public class SetupManager
    {
       
        private readonly CurrentInstallation _currentInstallation;
        private readonly SetupClientOperations _setupClientOperations;
        private readonly SetupDataCenterOperations _setupDataCenterOperations;
        private readonly SetupRtuManagerOperations _setupRtuManagerOperations;
        private readonly SetupUninstallOperations _setupUninstallOperations;

        public SetupManager(CurrentInstallation currentInstallation, SetupClientOperations setupClientOperations,
            SetupDataCenterOperations setupDataCenterOperations, SetupRtuManagerOperations setupRtuManagerOperations,
            SetupUninstallOperations setupUninstallOperations)
        {
            _currentInstallation = currentInstallation;
            _setupClientOperations = setupClientOperations;
            _setupDataCenterOperations = setupDataCenterOperations;
            _setupRtuManagerOperations = setupRtuManagerOperations;
            _setupUninstallOperations = setupUninstallOperations;
        }

        public bool Run(ObservableCollection<string> progressLines)
        {
            switch (_currentInstallation.InstallationType)
            {
                case InstallationType.Client:
                    if (!_setupClientOperations.SetupClient(progressLines, _currentInstallation.InstallationFolder))
                        return false;
                    break;
                case InstallationType.Datacenter:
                    if (!_setupDataCenterOperations.SetupDataCenter(progressLines, _currentInstallation.InstallationFolder))
                        return false;
                    break;
                case InstallationType.RtuManager:
                    if (!_setupRtuManagerOperations.SetupRtuManager(progressLines, _currentInstallation.InstallationFolder))
                        return false;
                    break;
            }

            _setupUninstallOperations.SetupUninstall(progressLines, _currentInstallation.InstallationFolder);
            return true;
        }
    }
}
