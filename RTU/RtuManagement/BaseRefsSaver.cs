using System;
using System.IO;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public class BaseRefsSaver
    {
        private readonly IMyLog _logFile;

        public BaseRefsSaver(IMyLog logFile)
        {
            _logFile = logFile;
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
            _logFile.AppendLine($"{fullPath}");
            return fullPath;
        }

        private string GetAbsolutePortFolder(OtauPortDto otauPortDto)
        {
            var baseFolder = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            var fullFolderName = Path.Combine(baseFolder, GetPortFolder(otauPortDto));
            if (!Directory.Exists(fullFolderName))
                Directory.CreateDirectory(fullFolderName);

            return fullFolderName;
        }

        private string GetPortFolder(OtauPortDto otauPortDto)
        {
            return $@"PortData\{otauPortDto.Serial}p{otauPortDto.OpticalPort:000}\";
        }
    }
}
