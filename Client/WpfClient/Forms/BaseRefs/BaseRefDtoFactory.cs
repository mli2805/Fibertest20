using System;
using System.IO;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class BaseRefDtoFactory
    {
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;

        public BaseRefDtoFactory(IMyLog logFile, CurrentUser currentUser)
        {
            _logFile = logFile;
            _currentUser = currentUser;
        }

        public BaseRefDto Create(string filename, BaseRefType type)
        {
            return filename != "" 
                ? GetBaseRefDto(filename, type) 
                : new BaseRefDto() { Id = Guid.Empty, BaseRefType = type};
        }

        private BaseRefDto GetBaseRefDto(string filename, BaseRefType type)
        {
            var bytes = File.ReadAllBytes(filename);

            var str = SorData.TryGetFromBytes(bytes, out var otdrDataKnownBlocks);
            if (string.IsNullOrEmpty(str))
                return new BaseRefDto()
                {
                    Id = Guid.NewGuid(),
                    BaseRefType = type,
                    UserName = _currentUser.UserName,
                    SaveTimestamp = DateTime.Now,
                    SorBytes = bytes,
                    Duration = TimeSpan.FromSeconds((int) otdrDataKnownBlocks.FixedParameters.AveragingTime)
                };

            _logFile.AppendLine(str);
            return new BaseRefDto() { Id = Guid.Empty, BaseRefType = type };
        }

     

    }
}