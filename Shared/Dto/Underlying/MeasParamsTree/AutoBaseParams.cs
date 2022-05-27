﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

        // AFTER measurement
        // Client requests index of template of parameters by lmax from received sorData
        // lmax in sorData could be slightly less than lmax in template
        public static int GetTemplateIndexByLmaxInSorData(double lmax, string omid)
        {
            var is4100 = omid == "RXT-4100+/1650 50dB";
            var list = is4100 ? Rxt4100Lmax.Select(double.Parse).ToList() : Lmax.Select(double.Parse).ToList();

            if (lmax < list[0]) return 0;

            for (int i = 1; i < list.Count; i++)
            {
                var halfDiff = (list[i] - list[i - 1]) / 2;
                if (lmax < list[i - 1] + halfDiff)
                    return i - 1;
            }

            return list.Count - 1;
        }


        public static VeexMeasOtdrParameters GetPredefinedParamsForLmax(double lmax, string omid)
        {
            var is4100 = omid == "RXT-4100+/1650 50dB";
            var index = GetIndexByProbeMeasurementLmax(lmax, omid);
            if (index == -1) return null;

            return new VeexMeasOtdrParameters
            {
                distanceRange = is4100 ? Rxt4100Lmax[index] : Lmax[index],
                resolution = is4100 ? Rxt4100Dl[index] : Dl[index],
                pulseDuration = is4100 ? Rxt4100Tp[index] : Tp[index],
                averagingTime = Time[index]
            };
        }

        // BEFORE measurement
        // RTU choose index of template of parameters for measurement by LMAX from probe request
        // so now we choose first template with lmax bigger than in sorData
        private static int GetIndexByProbeMeasurementLmax(double lmax, string omid)
        {
            var is4100 = omid == "RXT-4100+/1650 50dB";

            // current culture on RTU could be a culture with COMMA as a NumberDecimalSeparator (i.e. Russian or German)
            // while our strings use POINT
            var pointCulture = new CultureInfo("en") { NumberFormat = { NumberDecimalSeparator = "." } };
            var list = is4100
                ? Rxt4100Lmax.Select(s => double.Parse(s, pointCulture)).ToList()
                : Lmax.Select(s => double.Parse(s, pointCulture)).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                if (lmax <= list[i])
                    return i;
            }
            return -1;
        }

    }
}
