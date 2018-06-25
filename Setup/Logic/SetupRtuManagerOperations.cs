using System.Collections.ObjectModel;
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

        public int SetupRtuManager(ObservableCollection<string> progressLines, string installationFolder)
        {
            var fullRtuManagerPath = Path.Combine(installationFolder, RtuManagerSubdir);
            progressLines.Add("RTU Manager setup started.");

            if (!ServiceOperations.UninstallServiceIfExist(RtuWatchdogServiceName, RtuWatchdogDisplayName, progressLines))
                return -1;
            if (!ServiceOperations.UninstallServiceIfExist(RtuManagerServiceName, RtuManagerDisplayName, progressLines))
                return -1;

            var count = FileOperations.DirectoryCopyWithDecorations(SourcePathDatacenter, fullRtuManagerPath, progressLines);
            if (count == -1)
                return -1;

            var filename = Path.Combine(fullRtuManagerPath, RtuServiceFilename);
            if (!ServiceOperations.InstallService(RtuManagerServiceName,
                RtuManagerDisplayName, RtuManagerServiceDescription, filename, progressLines)) return -1;

            filename = Path.Combine(fullRtuManagerPath, RtuWatchdogServiceFilename);
            if (!ServiceOperations.InstallService(RtuWatchdogServiceName,
                RtuWatchdogDisplayName, RtuWatchdogServiceDescription, filename, progressLines)) return -1;

            progressLines.Add("RTU Manager setup completed successfully.");
            return count;
        }
    }
}