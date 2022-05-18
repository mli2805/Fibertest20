using System.Collections.Generic;
using System.ComponentModel;
using Iit.Fibertest.Dto;
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

        public static List<OtdrParametersTemplate> Get(Rtu rtu)
        {
            var is4100 = rtu.Omid == "RXT-4100+/1650 50dB";

            var result = new List<OtdrParametersTemplate>();
            result.Add(new OtdrParametersTemplate()
            {
                Id = 0,
                Title = "Auto Lmax definition",
            });

            for (int i = 0; i < 4; i++)
            {
                result.Add(new OtdrParametersTemplate()
                {
                    Id = i + 1,
                    Title = Titles[i],
                    Lmax = is4100 ? AutoBaseParams.Rxt4100Lmax[i] : AutoBaseParams.Lmax[i],
                    Dl = is4100 ? AutoBaseParams.Rxt4100Dl[i] : AutoBaseParams.Dl[i],
                    Tp = is4100 ? AutoBaseParams.Rxt4100Tp[i] : AutoBaseParams.Tp[i],
                    Time = AutoBaseParams.Time[i],
                });
            }

           
            return result;
        }
    }
}