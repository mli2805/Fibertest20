using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Setup
{
    public class SetupRtuManagerOperations
    {
        private const string RtuManagerServiceName = "FibertestRtuService";
        private const string RtuManagerDisplayName = "Fibertest 2.0 RTU Manager";
        private const string RtuManagerServiceDescription = "Fibertest 2.0 RTU Manager Service";

        private const string RtuWatchdogServiceName = "FibertestRtuWatchdog";
        private const string RtuWatchdogDisplayName = "Fibertest 2.0 RTU Watchdog";
        private const string RtuWatchdogServiceDescription = "Fibertest 2.0 RTU Watchdog Service";

        private const string SourcePathDatacenter = @"..\RtuFiles";
        private const string RtuManagerSubdir = @"RtuManager\bin";
        private const string RtuServiceFilename = @"Iit.Fibertest.RtuService.exe";
        private const string RtuWatchdogServiceFilename = @"Iit.Fibertest.RtuWatchdog.exe";

        public bool SetupRtuManager(BackgroundWorker worker, string installationFolder)
        {
            var fullRtuManagerPath = Path.Combine(installationFolder, RtuManagerSubdir);
            worker.ReportProgress(0, "RTU Manager setup started.");

            if (!ServiceOperations.UninstallServiceIfExist(RtuWatchdogServiceName, RtuWatchdogDisplayName, worker))
                return false;
            if (!ServiceOperations.UninstallServiceIfExist(RtuManagerServiceName, RtuManagerDisplayName, worker))
                return false;

            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathDatacenter, fullRtuManagerPath, worker))
                return false;

            var filename = Path.Combine(fullRtuManagerPath, RtuServiceFilename);
            if (!ServiceOperations.InstallService(RtuManagerServiceName,
                RtuManagerDisplayName, RtuManagerServiceDescription, filename, worker)) return false;

            filename = Path.Combine(fullRtuManagerPath, RtuWatchdogServiceFilename);
            if (!ServiceOperations.InstallService(RtuWatchdogServiceName,
                RtuWatchdogDisplayName, RtuWatchdogServiceDescription, filename, worker)) return false;

            worker.ReportProgress(0, "RTU Manager setup completed successfully.");
            return true;
        }
    }
}