using Iit.Fibertest.Dto;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Graph
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

        public static string ToSid(this RftsLevelType level)
        {
            switch (level)
            {
                case RftsLevelType.Minor: return @"SID_Minor";
                case RftsLevelType.Major: return @"SID_Major";
                case RftsLevelType.Critical: return @"SID_Critical";
                default: return @"SID_User_s";
            }
        }
    }
}