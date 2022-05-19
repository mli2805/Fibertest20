using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public static class AutoBaseParams
    {
        public static readonly List<string> Rxt4100Lmax = new List<string>() { "6.0", "10", "20", "40" };
        public static readonly List<string> Lmax = new List<string>() { "5.0", "10", "20", "40" };

        public static readonly List<string> Rxt4100Dl = new List<string>() { "0.26", "0.51", "0.51", "1.0" };
        public static readonly List<string> Dl = new List<string>() { "0.16", "0.32", "0.64", "1.3" };

        public static readonly List<string> Rxt4100Tp = new List<string>() { "10", "25", "25", "100" };
        public static readonly List<string> Tp = new List<string>() { "12", "25", "25", "100" };

        public static readonly List<string> Time = new List<string>() { "00:05", "00:15", "00:15", "00:15", };

        public static VeexMeasOtdrParameters GetPredefinedParamsForLmax(double lmax, string omid)
        {
            var ourLmax = ToOurLmax(lmax);
            if (ourLmax == null) return null;

            var is4100 = omid == "RXT-4100+/1650 50dB";
            var index = is4100 ? Rxt4100Lmax.IndexOf(ourLmax) : Lmax.IndexOf(ourLmax);

            return new VeexMeasOtdrParameters
            {
                distanceRange = ourLmax,
                resolution = is4100 ? Rxt4100Dl[index] : Dl[index],
                pulseDuration = is4100 ? Rxt4100Tp[index] : Tp[index],
                averagingTime = Time[index]
            };
        }

        private static string ToOurLmax(double lmax)
        {
            if (lmax <= 5)
                return "5.0";
            if (lmax <= 10)
                return "10";
            if (lmax <= 20)
                return "20";
            if (lmax <= 40)
                return "40";
            return null;
        }

    }
}
