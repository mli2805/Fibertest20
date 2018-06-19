using System.Collections.ObjectModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Setup
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
        private const string TargetPathDatacenter = @"C:\IIT-Fibertest\RtuManager\bin";
        private const string RtuServiceFilename = @"Iit.Fibertest.RtuService.exe";
        private const string RtuWatchdogServiceFilename = @"Iit.Fibertest.RtuWatchdog.exe";

        public bool SetupRtuManager(ObservableCollection<string> progressLines)
        {
            progressLines.Add("RTU Manager setup started.");

            if (!ServiceOperations.UninstallServiceIfNeeded(RtuWatchdogServiceName, RtuWatchdogDisplayName, progressLines))
                return false;
            if (!ServiceOperations.UninstallServiceIfNeeded(RtuManagerServiceName, RtuManagerDisplayName, progressLines))
                return false;

            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathDatacenter, TargetPathDatacenter, progressLines))
                return false;

            var filename = Path.Combine(TargetPathDatacenter, RtuServiceFilename);
            if (!ServiceOperations.InstallService(RtuManagerServiceName,
                RtuManagerDisplayName, RtuManagerServiceDescription, filename, progressLines)) return false;

            filename = Path.Combine(TargetPathDatacenter, RtuWatchdogServiceFilename);
            if (!ServiceOperations.InstallService(RtuWatchdogServiceName,
                RtuWatchdogDisplayName, RtuWatchdogServiceDescription, filename, progressLines)) return false;

            progressLines.Add("RTU Manager setup completed successfully.");
            return true;
        }
    }
}