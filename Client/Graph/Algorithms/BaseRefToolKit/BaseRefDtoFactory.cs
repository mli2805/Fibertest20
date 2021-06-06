using System;
using System.IO;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class BaseRefDtoFactory
    {
        public BaseRefDto CreateFromFile(string filename, BaseRefType type, string username)
        {
            if (filename == "")
                return new BaseRefDto() { Id = Guid.Empty, BaseRefType = type }; // delete old base ref

            var bytes = File.ReadAllBytes(filename);
            return new BaseRefDto()
            {
                Id = Guid.NewGuid(),
                BaseRefType = type,
                UserName = username,
                //  SaveTimestamp = DateTime.Now,  // will be set on server
                SorBytes = bytes,
                //   Duration = TimeSpan.FromSeconds((int) otdrDataKnownBlocks.FixedParameters.AveragingTime)  // will be set on server
            };
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