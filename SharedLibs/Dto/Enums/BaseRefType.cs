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
                case BaseRefType.Additional: return Resources.SID_Additional;
                case BaseRefType.None:
                default: return "";
            }
        }
    }
}