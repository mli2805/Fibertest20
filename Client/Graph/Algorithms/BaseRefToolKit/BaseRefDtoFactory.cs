using System;
using System.IO;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class BaseRefDtoFactory
    {
        private readonly IMyLog _logFile;

        public BaseRefDtoFactory(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public BaseRefDto CreateFromFile(string filename, BaseRefType type, string username)
        {
            return filename != "" 
                ? GetIt(filename, type, username) 
                : new BaseRefDto() { Id = Guid.Empty, BaseRefType = type}; // delete old base ref
        }

        private BaseRefDto GetIt(string filename, BaseRefType type, string username)
        {
            var bytes = File.ReadAllBytes(filename);

            var str = SorData.TryGetFromBytes(bytes, out var otdrDataKnownBlocks);
            if (string.IsNullOrEmpty(str))
                return new BaseRefDto()
                {
                    Id = Guid.NewGuid(),
                    BaseRefType = type,
                    UserName = username,
                    SaveTimestamp = DateTime.Now,
                    SorBytes = bytes,
                    Duration = TimeSpan.FromSeconds((int) otdrDataKnownBlocks.FixedParameters.AveragingTime)
                };

            _logFile.AppendLine(str);
            return new BaseRefDto() { Id = Guid.NewGuid(), BaseRefType = type };
        }

        public BaseRefDto CreateFromBaseRef(BaseRef baseRef, byte[] sorBytes)
        {
            return new BaseRefDto()
            {
                Id = baseRef.Id,
                BaseRefType = baseRef.BaseRefType,
                Duration = baseRef.Duration,
                SaveTimestamp = baseRef.SaveTimestamp,
                UserName = baseRef.UserName,
                SorFileId = baseRef.SorFileId,

                SorBytes = sorBytes,
            };
        }

    }
}