using Iit.Fibertest.Dto;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.UtilsLib
{
    public static class RftsLevelTypeExt
    {
        public static FiberState ConvertToFiberState(this RftsLevelType level)
        {
            switch (level)
            {
                case RftsLevelType.Minor: return FiberState.Minor;
                case RftsLevelType.Major: return FiberState.Major;
                case RftsLevelType.Critical: return FiberState.Critical;
                default: return FiberState.User;
            }
        }
    }
}