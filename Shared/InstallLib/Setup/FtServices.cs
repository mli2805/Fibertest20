using System.Collections.Generic;

namespace Iit.Fibertest.InstallLib
{
    public static class FtServices
    {
        public static readonly List<FtService> List= new List<FtService>
        {
            new FtService("FibertestDcService", "Fibertest 2.0 DataCenter Server Service")
            {
                Ordinal = 1,
                SourcePath = @"..\DcFiles", 
                FolderInsideFibertest =  @"DataCenter", 
                FolderForBinaries = @"bin", 
                MainExe = @"Iit.Fibertest.DataCenterService.exe",
                DestinationComputer = DestinationComputer.DataCenter,
            }, 
            new FtService("FibertestWaService", "Fibertest 2.0 DataCenter WebApi Service")
            {
                Ordinal = 0, 
                SourcePath = @"..\WebApi", 
                FolderInsideFibertest =  @"WebApi", 
                FolderForBinaries = @"publish", 
                MainExe = @"Iit.Fibertest.DataCenterWebApi.exe",
                DestinationComputer = DestinationComputer.DataCenter,
            },
            new FtService("FibertestRtuService", "Fibertest 2.0 RTU Manager Service")
            {
                Ordinal = 1,
                SourcePath = @"..\RtuFiles", 
                FolderInsideFibertest =  @"RtuManager", 
                FolderForBinaries = @"bin", 
                MainExe = @"Iit.Fibertest.RtuService.exe",
                DestinationComputer = DestinationComputer.Rtu,
            },
            new FtService("FibertestRtuWatchdog", "Fibertest 2.0 RTU Watchdog Service")
            {
                Ordinal = 0, // should be uninstalled first
                SourcePath = @"..\RtuFiles", 
                FolderInsideFibertest =  @"RtuManager", 
                FolderForBinaries = @"bin", 
                MainExe = @"Iit.Fibertest.RtuWatchdog.exe",
                DestinationComputer = DestinationComputer.Rtu,
            },
        };
    }
}