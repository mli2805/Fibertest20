using System;
using System.IO;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public class BaseRefsSaver
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

//        private readonly string _appDir;

        public BaseRefsSaver(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;

//            _appDir = TryGetAppFolder() ?? @"c:\";
        }

        public ReturnCode SaveBaseRefs(AssignBaseRefsDto dto)
        {
            var fullFolderName = GetAbsolutePortFolder(dto.OtauPortDto);

            foreach (var baseRef in dto.BaseRefs)
                RemoveOldSaveNew(baseRef, fullFolderName);
            return ReturnCode.BaseRefAssignedSuccessfully;
        }

        private void RemoveOldSaveNew(BaseRefDto baseRef, string fullFolderName)
        {
            var fullPath = BuildFullPath(baseRef.BaseRefType, fullFolderName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            if (baseRef.SorBytes != null)
                File.WriteAllBytes(fullPath, baseRef.SorBytes);
        }

        private string BuildFullPath(BaseRefType baseRefType, string fullFolderName)
        {
            var filename = baseRefType.ToBaseFileName();
            var fullPath = Path.Combine(fullFolderName, filename);
            _logFile.AppendLine($"with name: {fullPath}");
            return fullPath;
        }

        private string GetAbsolutePortFolder(OtauPortDto otauPortDto)
        {
//            var fullFolderName = Path.Combine(_appDir, @"..\PortData\", GetPortFolder(otauPortDto));
            var fullFolderName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\PortData\", GetPortFolder(otauPortDto));
            if (!Directory.Exists(fullFolderName))
                Directory.CreateDirectory(fullFolderName);

            return fullFolderName;
        }

        private string GetPortFolder(OtauPortDto otauPortDto)
        {
            var otdrIp = _iniFile.Read(IniSection.RtuManager, IniKey.OtdrIp, "192.168.88.101");
            return otauPortDto.IsPortOnMainCharon
                ? $@"{otdrIp}t{otauPortDto.OtauTcpPort}p{otauPortDto.OpticalPort}\"
                : $@"{otauPortDto.OtauIp}t{otauPortDto.OtauTcpPort}p{otauPortDto.OpticalPort}\";
        }

//        private string TryGetAppFolder()
//        {
//            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
//            return Path.GetDirectoryName(appPath);
//        }

    }
}
