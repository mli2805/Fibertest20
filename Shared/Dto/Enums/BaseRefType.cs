using System;

namespace Iit.Fibertest.Dto
{
    public enum BaseRefType
    {
        None,
        Precise,
        Fast,
        Additional
    }

    public static class BaseRefExt
    {
        public static string ToFileName(this BaseRefType baseRefType, SorType sorType)
        {
            var dt = "";
            if (sorType == SorType.Error) dt = $"_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
            switch (baseRefType)
            {
                case BaseRefType.Precise:
                    return $"Precise_{sorType}{dt}.sor";
                case BaseRefType.Fast:
                    return $"Fast_{sorType}{dt}.sor";
                case BaseRefType.Additional:
                    return $"Additional_{sorType}{dt}.sor";
                default:
                    return "";
            }
        }
        public static string ToBaseFileName(this BaseRefType baseRefType)
        {
            return ToFileName(baseRefType, SorType.Base);
        }
    }

}