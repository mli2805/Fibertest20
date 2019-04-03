using System.IO;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class DiskSpaceProvider
    {
        private readonly IEventStoreInitializer _mySqlEventStoreInitializer;
        private readonly IMyLog _logFile;

        private readonly int _freeSpaceThresholdGb;

        public DiskSpaceProvider(IEventStoreInitializer mySqlEventStoreInitializer, IniFile iniFile, IMyLog logFile)
        {
            _mySqlEventStoreInitializer = mySqlEventStoreInitializer;
            _logFile = logFile;

            _freeSpaceThresholdGb = iniFile.Read(IniSection.MySql, IniKey.FreeSpaceThresholdGb, 100);
        }

        public async Task<DiskSpaceDto>  GetDiskSpace()
        {
            var result = new DiskSpaceDto();
            var driveInfo = new DriveInfo(_mySqlEventStoreInitializer.DataDir.Substring(0,1));
            result.TotalSize = driveInfo.TotalSize;
            result.AvailableFreeSpace = driveInfo.AvailableFreeSpace;
            result.DataSize = 1;
            var totalSize = $"{driveInfo.TotalSize / (1024.0 * 1024 * 1024):#.0}Gb";
            var freeSpace = $"{driveInfo.AvailableFreeSpace / (1024.0 * 1024 * 1024):#.0}Gb";
            _logFile.AppendLine($@"Drive {driveInfo.Name}'s size is {totalSize},  free space is {freeSpace}");
            result.DataSize = _mySqlEventStoreInitializer.GetDataSize();
            result.FreeSpaceThreshold = _freeSpaceThresholdGb;
            await Task.Delay(1);
            return result;
        }
    }
}