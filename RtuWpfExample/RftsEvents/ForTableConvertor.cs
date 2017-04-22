using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.RtuWpfExample
{
    public static class ForTableConvertor
    {
        public static string ForTable(this RftsEventTypes rftsEventType)
        {
            switch (rftsEventType)
            {
                case RftsEventTypes.None: return Resources.SID_no;
                case RftsEventTypes.IsMonitored: return Resources.SID_yes;
            }
            return Resources.SID_unexpected_input;
        }

        public static string ForTable(this LandmarkCode landmarkCode)
        {
            switch (landmarkCode)
            {
                case LandmarkCode.FiberDistributingFrame: return Resources.SID_Rtu;
                case LandmarkCode.Coupler: return Resources.SID_Closure;
                case LandmarkCode.WiringCloset: return Resources.SID_Cross;
                case LandmarkCode.Manhole: return Resources.SID_Node;
                case LandmarkCode.RemoteTerminal: return Resources.SID_Terminal;
                case LandmarkCode.Other: return Resources.SID_Other;
            }
            return Resources.SID_unexpected_input;
        }

        public static string ForTable(this ShortThreshold threshold)
        {
            var value = threshold.IsAbsolute ? threshold.AbsoluteThreshold : threshold.RelativeThreshold;
            var str = $@"{value / 1000.0 : 0.000} ";
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