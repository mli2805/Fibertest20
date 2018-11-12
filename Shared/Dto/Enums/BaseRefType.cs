﻿using System;
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

    public enum SorType
    {
        Base,
        Meas,
        Raw,   // before any processing
        Error, // error while measurement
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

        public static string ToMeasFileName(this BaseRefType baseRefType)
        {
            return ToFileName(baseRefType, SorType.Meas);
        }
    }
}