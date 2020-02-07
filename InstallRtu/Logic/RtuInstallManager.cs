using System.ComponentModel;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.InstallRtu
{
    public class RtuInstallManager
    {

        private readonly CurrentRtuInstallation _currentRtuInstallation;
        private readonly IMyLog _logFile;
        private readonly SetupRtuManagerOperations _setupRtuManagerOperations;
        private readonly SetupUninstallOperations _setupUninstallOperations;

        public RtuInstallManager(CurrentRtuInstallation currentRtuInstallation, IMyLog logFile,
            SetupRtuManagerOperations setupRtuManagerOperations,
            SetupUninstallOperations setupUninstallOperations)
        {
            _currentRtuInstallation = currentRtuInstallation;
            _logFile = logFile;
            _setupRtuManagerOperations = setupRtuManagerOperations;
            _setupUninstallOperations = setupUninstallOperations;
        }

        public bool Run(BackgroundWorker worker)
        {
            _logFile.AppendLine("Setup process started...");

            if (!_setupRtuManagerOperations.SetupRtuManager(worker, _currentRtuInstallation.InstallationFolder))
                return false;
          
            _logFile.AppendLine("Setup uninstall application");
            _setupUninstallOperations.SetupUninstall(worker, _currentRtuInstallation.InstallationFolder);
            return true;
        }
    }
}
