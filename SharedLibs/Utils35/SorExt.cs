using System;
using System.IO;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.UtilsLib
{
    public static class SorExt
    {
        public static BaseRefDto GetBaseRefDto(string filename, BaseRefType type)
        {
            var guid = filename != "" ? Guid.NewGuid() : Guid.Empty;
            var content = filename != "" ? File.ReadAllBytes(filename) : null;
            // TODO get duration from sor file
            var duration = filename != "" ? TimeSpan.FromSeconds(33) :  TimeSpan.Zero;
            return new BaseRefDto() { Id = guid, BaseRefType = type, SorBytes = content, Duration = duration};
        }

    }
}