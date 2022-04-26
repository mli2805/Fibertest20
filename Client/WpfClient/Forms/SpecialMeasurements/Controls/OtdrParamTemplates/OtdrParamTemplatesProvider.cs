using System.Collections.Generic;
using System.ComponentModel;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    [Localizable(false)]
    public static class OtdrParamTemplatesProvider
    {
        public static List<OtdrParametersTemplate> Get(Rtu rtu)
        {
            return new List<OtdrParametersTemplate>()
            {
                new OtdrParametersTemplate()
                {
                    Title = Resources.SID__1__For_traces_0_05___0_5_km_long,
                    Lmax = rtu.Omid == "RXT-4100+/1650 50dB" ? "6.0" : "5.0",
                    Dl = "0.16",
                    Tp = rtu.Omid == "RXT-4100+/1650 50dB" ? "10" : "12",
                    Time = "00:05",
                },
                new OtdrParametersTemplate()
                {
                    Title = Resources.SID__2__For_traces_0_5___5_km_long,
                    Lmax = "10",
                    Dl = "0.32",
                    Tp = "25",
                    Time = "00:15",

                    IsChecked = true,
                },
                new OtdrParametersTemplate()
                {
                    Title = Resources.SID__3__For_traces_5___10_km_long,
                    Lmax = "20",
                    Dl = "0.64",
                    Tp = "25",
                    Time = "00:15",
                },
                new OtdrParametersTemplate()
                {
                    Title = Resources.SID__4__For_traces_10___20_km_long,
                    Lmax = "40",
                    Dl = "1.3",
                    Tp = "100",
                    Time = "00:15",
                },
            };
        }
    }
}