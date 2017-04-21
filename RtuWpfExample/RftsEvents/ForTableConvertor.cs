using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace RtuWpfExample
{
    public static class ForTableConvertor
    {
        public static string ForTable(this RftsEventTypes rftsEventType)
        {
            switch (rftsEventType)
            {
                case RftsEventTypes.None: return "no";
                case RftsEventTypes.IsMonitored: return "yes";
            }
            return "unexpected input";
        }

        public static string ForTable(this LandmarkCode landmarkCode)
        {
            switch (landmarkCode)
            {
                case LandmarkCode.FiberDistributingFrame: return "RTU";
                case LandmarkCode.Coupler: return "Sleeve";
                case LandmarkCode.RemoteTerminal: return "Terminal";
            }
            return "unexpected input";
        }

        public static string ForTable(this ShortThreshold threshold)
        {
            var value = threshold.IsAbsolute ? threshold.AbsoluteThreshold : threshold.RelativeThreshold;
            var str = $"{value / 1000.0 : 0.000}";
            var result = str + (threshold.IsAbsolute ? " (abs.)" : " (rel.)");
            return result;
        }

        public static string ForTable(this ShortDeviation deviation)
        {
            var value = (deviation.Type & ShortDeviationTypes.IsCompared) != 0;
            return value ? "1" : "2";
        }
    }
}