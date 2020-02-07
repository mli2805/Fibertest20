﻿using System.ComponentModel;
using System.IO;
using System.Threading;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.InstallRtu
{
    public class SetupRtuManagerOperations
    {
        private const string RtuManagerServiceName = "FibertestRtuService";
        private const string RtuManagerDisplayName = "Fibertest 2.0 RTU Manager";
        private const string RtuManagerServiceDescription = "Fibertest 2.0 RTU Manager Service";

        private const string RtuWatchdogServiceName = "FibertestRtuWatchdog";
        private const string RtuWatchdogDisplayName = "Fibertest 2.0 RTU Watchdog";
        private const string RtuWatchdogServiceDescription = "Fibertest 2.0 RTU Watchdog Service";

        private const string SourcePathRtuManager = @"..\RtuFiles";
        private const string RtuManagerSubdir = @"RtuManager\bin";
        private const string RtuManagerIniSubdir = @"RtuManager\ini";
        private const string SourcePathUtils = @"..\Utils";
        private const string UtilsSubdir = @"Utils";
        private const string SourcePathReflect = @"..\RftsReflect";
        private const string ReflectSubdir = @"RftsReflect";
        private const string RtuServiceFilename = @"Iit.Fibertest.RtuService.exe";
        private const string RtuWatchdogServiceFilename = @"Iit.Fibertest.RtuWatchdog.exe";

        public bool SetupRtuManager(BackgroundWorker worker, string installationFolder)
        {
            var fullRtuManagerPath = Path.Combine(installationFolder, RtuManagerSubdir);
            var fullUtilsPath = Path.Combine(installationFolder, UtilsSubdir);
            var fullReflectPath = Path.Combine(installationFolder, ReflectSubdir);
            worker.ReportProgress((int)BwReturnProgressCode.RtuManagerSetupStarted);

            if (!ServiceOperations.UninstallServiceIfExist(RtuWatchdogServiceName, RtuWatchdogDisplayName, worker))
                return false;
            if (!ServiceOperations.UninstallServiceIfExist(RtuManagerServiceName, RtuManagerDisplayName, worker))
                return false;

            Thread.Sleep(1000);
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopied);
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathRtuManager, true, fullRtuManagerPath, worker))
                return false;

            var otdrmeasengine = Path.Combine(fullRtuManagerPath, @"OtdrMeasEngine\");
            FileOperations.CleanAntiGhost(otdrmeasengine, true);
            CreateIniForIpAddressesSetting(installationFolder);

            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathUtils, true, 
                fullUtilsPath, worker))
                return false;

            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathReflect, true,
                fullReflectPath, worker))
                return false;
            FileOperations.CleanAntiGhost(fullReflectPath, false);

            if (!Directory.Exists(fullReflectPath + "\\Share"))
                Directory.CreateDirectory(fullReflectPath + "\\Share");
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);

            var filename = Path.Combine(fullRtuManagerPath, RtuServiceFilename);
            if (!ServiceOperations.InstallService(RtuManagerServiceName,
                RtuManagerDisplayName, RtuManagerServiceDescription, filename, worker)) return false;

            filename = Path.Combine(fullRtuManagerPath, RtuWatchdogServiceFilename);
            if (!ServiceOperations.InstallService(RtuWatchdogServiceName,
                RtuWatchdogDisplayName, RtuWatchdogServiceDescription, filename, worker)) return false;

            worker.ReportProgress((int)BwReturnProgressCode.RtuManagerSetupCompletedSuccessfully);
            return true;
        }

        private void CreateIniForIpAddressesSetting(string installationFolder)
        {
            var iniRtuManagerPath = Path.Combine(installationFolder, RtuManagerIniSubdir);

            var iniFile = new IniFile();
            var iniFileName = Utils.FileNameForSure(iniRtuManagerPath, "RtuManager.ini",
                false, true);
            iniFile.AssignFile(iniFileName, true);

            iniFile.Read(IniSection.RtuManager, IniKey.OtdrIp, "192.168.88.101");
            iniFile.Read(IniSection.RtuManager, IniKey.OtauIp, "192.168.88.101");
        }

    }
}