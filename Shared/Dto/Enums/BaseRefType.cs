using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Dto
{
    public enum BaseRefType
    {
        None,
        Precise,
        Fast,
        Additional
    }

    public static class BaseRefTypeExt
    {
        public static string GetLocalizedString(this BaseRefType baseRefType)
        {
            switch (baseRefType)
            {
                case BaseRefType.Precise: return Resources.SID_Precise;
                case BaseRefType.Fast: return Resources.SID_Fast;
                case BaseRefType.Additional: return Resources.SID_Second;
                default: return "";
            }
        }
        public static string GetLocalizedFemaleString(this BaseRefType baseRefType)
        {
            switch (baseRefType)
            {
                case BaseRefType.Precise: return Resources.SID_PreciseF;
                case BaseRefType.Fast: return Resources.SID_FastF;
                case BaseRefType.Additional: return Resources.SID_SecondF;
                default: return "";
            }
        }

        private static string ToFileName(this BaseRefType baseRefType, string prefix)
        {
            switch (baseRefType)
            {
                case BaseRefType.Precise:
                    return prefix + "Precise.sor";
                case BaseRefType.Fast:
                    return prefix + "Fast.sor";
                case BaseRefType.Additional:
                    return prefix + "Additional.sor";
                default:
                    return "";
            }
        }
        public static string ToBaseFileName(this BaseRefType baseRefType)
        {
            return ToFileName(baseRefType, "Base");
        }

        public static string ToMeasFileName(this BaseRefType baseRefType)
        {
            return ToFileName(baseRefType, "Meas");
        }
    }
}