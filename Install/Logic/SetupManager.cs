using System.ComponentModel;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class SetupManager
    {

        private readonly CurrentInstallation _currentInstallation;
        private readonly IMyLog _logFile;
        private readonly SetupClientOperations _setupClientOperations;
        private readonly SetupDataCenterOperations _setupDataCenterOperations;
        private readonly SetupSuperClientOperations _setupSuperClientOperations;
        private readonly SetupUninstallOperations _setupUninstallOperations;

        public SetupManager(CurrentInstallation currentInstallation, IMyLog logFile,
            SetupClientOperations setupClientOperations,
            SetupDataCenterOperations setupDataCenterOperations,
            SetupSuperClientOperations setupSuperClientOperations,
            SetupUninstallOperations setupUninstallOperations)
        {
            _currentInstallation = currentInstallation;
            _logFile = logFile;
            _setupClientOperations = setupClientOperations;
            _setupDataCenterOperations = setupDataCenterOperations;
            _setupSuperClientOperations = setupSuperClientOperations;
            _setupUninstallOperations = setupUninstallOperations;
        }

        public bool Run(BackgroundWorker worker)
        {
            _logFile.AppendLine("Setup process started...");
            switch (_currentInstallation.InstallationType)
            {
                case InstallationType.Client:
                    if (!_setupClientOperations.SetupClient(worker, _currentInstallation.InstallationFolder))
                        return false;
                    break;
                case InstallationType.Datacenter:
                    if (!_setupDataCenterOperations.SetupDataCenter(worker, _currentInstallation))
                        return false;
                    if (!_setupClientOperations.SetupClient(worker, _currentInstallation.InstallationFolder))
                        return false;
                    break;
                case InstallationType.SuperClient:
                    if (!_setupSuperClientOperations.SetupSuperClient(worker, _currentInstallation.InstallationFolder))
                        return false;
                    if (!_setupClientOperations.SetupClient(worker, _currentInstallation.InstallationFolder))
                        return false;
                    break;
            }

            _logFile.AppendLine("Setup uninstall application");
            _setupUninstallOperations.SetupUninstall(worker, _currentInstallation.InstallationFolder);
            return true;
        }
    }
}
