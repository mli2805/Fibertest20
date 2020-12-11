using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.InstallRtu
{
    public class SetupRtuManagerOperations
    {
        private const string SourcePathUtils = @"..\Utils";
        private const string UtilsSubdir = @"Utils";
        private const string SourcePathReflect = @"..\RftsReflect";
        private const string ReflectSubdir = @"RftsReflect";

        public bool SetupRtuManager(BackgroundWorker worker, string installationFolder)
        {
            worker.ReportProgress((int)BwReturnProgressCode.RtuManagerSetupStarted);

            if (!FtServices.List
                .Where(s => s.DestinationComputer == DestinationComputer.Rtu)
                .All(ss => ServiceOperations.UninstallServiceIfExist(ss, worker)))
                return false;

            Thread.Sleep(1000);
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreBeingCopied);

            var service = FtServices.List.First(s => s.Name == "FibertestRtuService");
            var fullBinariesFolder = service
                .GetFullBinariesFolder(installationFolder);
            // both service RtuManager and RtuWatchdog are in one folder
            if (!FileOperations.DirectoryCopyWithDecorations(service.SourcePath, fullBinariesFolder, worker))
                return false;

            var otdrmeasengine = Path.Combine(fullBinariesFolder, @"OtdrMeasEngine\");
            if (!FileOperations.CleanAntiGhost(otdrmeasengine, true, worker)) 
                return false;
            CreateIniForIpAddressesSetting(installationFolder);

            var fullUtilsPath = Path.Combine(installationFolder, UtilsSubdir);
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathUtils,  
                fullUtilsPath, worker))
                return false;

            if (!CopyReflect(Path.Combine(installationFolder, ReflectSubdir), worker)) 
                return false;
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreCopiedSuccessfully);

            if (!ServiceOperations.InstallSericesOnPc(DestinationComputer.Rtu,
                installationFolder, worker)) return false;

            worker.ReportProgress((int)BwReturnProgressCode.RtuManagerSetupCompletedSuccessfully);
            return true;
        }

        private bool CopyReflect(string fullReflectPath, BackgroundWorker worker)
        {
            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathReflect, 
                fullReflectPath, worker))
                return false;
            if (!FileOperations.CleanAntiGhost(fullReflectPath, false, worker)) 
                return false;

            if (!Directory.Exists(fullReflectPath + "\\Share"))
                Directory.CreateDirectory(fullReflectPath + "\\Share");
            return true;
        }

        private void CreateIniForIpAddressesSetting(string installationFolder)
        {
            var iniRtuManagerPath = Path.Combine(installationFolder, @"RtuManager\ini");

            var iniFile = new IniFile();
            var iniFileName = Utils.FileNameForSure(iniRtuManagerPath, "RtuManager.ini",
                false, true);
            iniFile.AssignFile(iniFileName, true);

            iniFile.Read(IniSection.RtuManager, IniKey.OtdrIp, "192.168.88.101");
            iniFile.Read(IniSection.RtuManager, IniKey.OtauIp, "192.168.88.101");
        }

    }
}