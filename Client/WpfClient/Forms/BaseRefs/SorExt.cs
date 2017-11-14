using System;
using System.IO;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class SorExt
    {
        private readonly IMyLog _logFile;

        public SorExt(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public BaseRefDto GetBaseRefDto(string filename, BaseRefType type)
        {
            return filename != "" 
                ? FullGetBaseRefDto(filename, type) 
                : new BaseRefDto() { Id = Guid.Empty, BaseRefType = type};
        }

        private BaseRefDto FullGetBaseRefDto(string filename, BaseRefType type)
        {
            var bytes = File.ReadAllBytes(filename);

            OtdrDataKnownBlocks otdrDataKnownBlocks;
            var str = SorData.TryGetFromBytes(bytes, out otdrDataKnownBlocks);
            if (string.IsNullOrEmpty(str))
                return new BaseRefDto()
                {
                    Id = Guid.NewGuid(),
                    BaseRefType = type,
                    SorBytes = bytes,
                    Duration = TimeSpan.FromSeconds((int) otdrDataKnownBlocks.FixedParameters.AveragingTime)
                };

            _logFile.AppendLine(str);
            return new BaseRefDto() { Id = Guid.Empty, BaseRefType = type };
        }

    }
}