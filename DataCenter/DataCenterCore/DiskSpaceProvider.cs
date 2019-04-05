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

        private readonly double _freeSpaceThresholdGb;

        public DiskSpaceProvider(IEventStoreInitializer mySqlEventStoreInitializer, IniFile iniFile, IMyLog logFile)
        {
            _mySqlEventStoreInitializer = mySqlEventStoreInitializer;
            _logFile = logFile;

            _freeSpaceThresholdGb = iniFile.Read(IniSection.MySql, IniKey.FreeSpaceThresholdGb, 100);
        }

        public async Task<DiskSpaceDto>  GetDiskSpaceGb()
        {
            const double gb = 1024.0 * 1024 * 1024;
            var result = new DiskSpaceDto();
            var driveInfo = new DriveInfo(_mySqlEventStoreInitializer.DataDir.Substring(0,1));
            result.TotalSize = driveInfo.TotalSize / gb;
            result.AvailableFreeSpace = driveInfo.AvailableFreeSpace / gb;
            var totalSize = $"{result.TotalSize:#.0}Gb";
            var freeSpace = $"{result.AvailableFreeSpace:#.0}Gb";
            result.FreeSpaceThreshold = _freeSpaceThresholdGb;
            result.DataSize = _mySqlEventStoreInitializer.GetDataSize() / gb;
            _logFile.AppendLine($@"Drive {driveInfo.Name}'s size is {totalSize},  free space is {freeSpace},  threshold {result.FreeSpaceThreshold}Gb");
            _logFile.AppendLine($@"DB size is {result.DataSize:#.0}Gb");
            await Task.Delay(1);
            return result;
        }
    }
}