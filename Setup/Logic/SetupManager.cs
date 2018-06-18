using System.Collections.ObjectModel;

namespace Setup
{
    public class SetupManager
    {
       
        private readonly CurrentInstallation _currentInstallation;
        private readonly SetupClientOperations _setupClientOperations;
        private readonly SetupDatacenterOperations _setupDatacenterOperations;
        private readonly SetupRtuManagerOperations _setupRtuManagerOperations;

        public SetupManager(CurrentInstallation currentInstallation, SetupClientOperations setupClientOperations,
            SetupDatacenterOperations setupDatacenterOperations, SetupRtuManagerOperations setupRtuManagerOperations)
        {
            _currentInstallation = currentInstallation;
            _setupClientOperations = setupClientOperations;
            _setupDatacenterOperations = setupDatacenterOperations;
            _setupRtuManagerOperations = setupRtuManagerOperations;
        }

        public bool Run(ObservableCollection<string> progressLines)
        {
            switch (_currentInstallation.InstallationType)
            {
                case InstallationType.Client:
                    _setupClientOperations.SetupClient(progressLines);
                    break;
                case InstallationType.Datacenter:
                    return _setupDatacenterOperations.SetupDataCenter(progressLines);
                case InstallationType.RtuManager:
                    _setupRtuManagerOperations.SetupRtuManager(progressLines);
                    break;
            }

            return false;
        }
    }
}
