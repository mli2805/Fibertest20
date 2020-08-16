using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.DataCenterCore
{
    public static class RftsEventsExt
    {
        public static string ForStateInTable(this RftsEventTypes rftsEventType, bool isFailed)
        {
            if ((rftsEventType & RftsEventTypes.IsFiberBreak) != 0)
                return "SID_fiber_break";
            if ((rftsEventType & RftsEventTypes.IsNew) != 0)
                return "SID_new";
            if ((rftsEventType & RftsEventTypes.IsFailed) != 0)
                return "SID_fail";
            if ((rftsEventType & RftsEventTypes.IsMonitored) != 0)
                return isFailed ? "SID_fail" : "SID_pass";
            if (rftsEventType == RftsEventTypes.None)
                return "";
            return "SID_unexpected_input";
        }
        public static string ForEnabledInTable(this RftsEventTypes rftsEventType)
        {
            if ((rftsEventType & RftsEventTypes.IsNew) != 0)
                return "SID_new";
            if ((rftsEventType & RftsEventTypes.IsMonitored) != 0)
                return "SID_yes";
            if (rftsEventType == RftsEventTypes.None)
                return "SID_pass";
            return "SID_unexpected_input";
        }

        public static string ForTable(this LandmarkCode landmarkCode)
        {
            switch (landmarkCode)
            {
                case LandmarkCode.FiberDistributingFrame: return "SID_Rtu";
                case LandmarkCode.Coupler: return "SID_Closure";
                case LandmarkCode.WiringCloset: return "SID_Cross";
                case LandmarkCode.Manhole: return "SID_Node";
                case LandmarkCode.RemoteTerminal: return "SID_Terminal";
                case LandmarkCode.Other: return "SID_Other";
            }
            return "SID_unexpected_input";
        }

        public static string ForTable(this ShortThreshold threshold)
        {
            var value = threshold.IsAbsolute ? threshold.AbsoluteThreshold : threshold.RelativeThreshold;
            var str = $@"{value / 1000.0: 0.000} ";
            var result = str + (threshold.IsAbsolute ? Resources.SID__abs__ : Resources.SID__rel__);
            return result;
        }

        public static string EventCodeForTable(this string eventCode)
        {
            var str = eventCode[0] == '0' ? @"S" : @"R";
            return $@"{str} : {eventCode[1]}";
        }


    }
}