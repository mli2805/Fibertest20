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
            result.FreeSpaceThreshold = _freeSpaceThresholdGb;
            result.DataSize = _mySqlEventStoreInitializer.GetDataSize() / gb;

            var totalSize = $"Drive {driveInfo.Name}'s size is {result.TotalSize:0.0}Gb";
            var freeSpace = $"free space is {result.AvailableFreeSpace:0.0}Gb";
            var threshold = $"threshold {result.FreeSpaceThreshold}Gb";
            var dbSize = $"DB size is {result.DataSize:0.0}Gb";
            _logFile.AppendLine($@"{totalSize},  {freeSpace},  {threshold},  {dbSize}");
            await Task.Delay(1);
            return result;
        }
    }
}