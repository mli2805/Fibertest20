using System.Collections.Generic;
using System.ComponentModel;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    [Localizable(false)]
    public static class OtdrParamTemplatesProvider
    {
        private static readonly List<string> Titles = new List<string>()
        {
            Resources.SID__1__For_traces_0_05___0_5_km_long,
            Resources.SID__2__For_traces_0_5___5_km_long,
            Resources.SID__3__For_traces_5___10_km_long,
            Resources.SID__4__For_traces_10___20_km_long,
        };

        private static readonly List<string> Rxt4100Lmax = new List<string>() { "6.0", "10", "20", "40" };
        private static readonly List<string> Lmax = new List<string>() { "5.0", "10", "20", "40" };

        private static readonly List<string> Rxt4100Dl = new List<string>() { "0.26", "0.51", "0.51", "1.0" };
        private static readonly List<string> Dl = new List<string>() { "0.16", "0.32", "0.64", "1.3" };

        private static readonly List<string> Rxt4100Tp = new List<string>() { "10", "25", "25", "25" };
        private static readonly List<string> Tp = new List<string>() { "12", "25", "25", "25" };

        private static readonly List<string> Time = new List<string>() { "00:05", "00:15", "00:15", "00:15", };

        public static List<OtdrParametersTemplate> Get(Rtu rtu)
        {
            var is4100 = rtu.Omid == "RXT-4100+/1650 50dB";

            var result = new List<OtdrParametersTemplate>();
            for (int i = 1; i <= 4; i++)
            {
                result.Add(new OtdrParametersTemplate()
                {
                    Id = i,
                    Title = Titles[i-1],
                    Lmax = is4100 ? Rxt4100Lmax[i-1] : Lmax[i-1],
                    Dl = is4100 ? Rxt4100Dl[i-1] : Dl[i-1],
                    Tp = is4100 ? Rxt4100Tp[i-1] : Tp[i-1],
                    Time = Time[i-1],
                });
            }

            return result;
        }
    }
}