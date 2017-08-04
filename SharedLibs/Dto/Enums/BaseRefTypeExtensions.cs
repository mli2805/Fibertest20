namespace Dto
{
    public static class BaseRefTypeExtensions
    {
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