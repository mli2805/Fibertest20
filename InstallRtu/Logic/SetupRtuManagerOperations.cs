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

            if (!ServiceOperations.UninstallAllServicesOnThisPc(worker))
                return false;

            Thread.Sleep(1000);
            worker.ReportProgress((int)BwReturnProgressCode.FilesAreBeingCopied);
            foreach (var service in FtServices.List.Where(s=>s.DestinationComputer == DestinationComputer.Rtu))
            {
                if (!FileOperations.DirectoryCopyWithDecorations(service.SourcePath,
                    service.GetFullBinariesFolder(installationFolder), worker))
                    return false;
            }

            var fullBinariesFolder = FtServices.List.First(s => s.Name == "RtuManager")
                .GetFullBinariesFolder(installationFolder);
            var otdrmeasengine = Path.Combine(fullBinariesFolder, @"OtdrMeasEngine\");
            FileOperations.CleanAntiGhost(otdrmeasengine, true);
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
            FileOperations.CleanAntiGhost(fullReflectPath, false);

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