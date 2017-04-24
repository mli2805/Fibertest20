using System;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class PointLatLngExtensions
    {
        public static string ToDetailedString(this PointLatLng pointLatLng, GpsInputMode mode)
        {
            string degreeSign = Resources.SID_Degree_sign;
            string minuteSign = Resources.SID_Minute_sign;
            string secondSign = Resources.SID_Second_sign;
            if (mode == GpsInputMode.Degrees)
            {
                return $@"{pointLatLng.Lat:#0.000000}{degreeSign} {pointLatLng.Lng:#0.000000}{degreeSign}";
            }
            if (mode == GpsInputMode.DegreesAndMinutes)
            {
                int dLat = (int)pointLatLng.Lat;
                double mLat = (pointLatLng.Lat - dLat) * 60;

                int dLng = (int)pointLatLng.Lng;
                double mLng = (pointLatLng.Lng - dLng) * 60;

                return $@"{dLat:#0}{degreeSign} {mLat:#0.0000}{minuteSign}  {dLng:#0}{degreeSign} {mLng:#0.0000}{minuteSign}";
            }
            if (mode == GpsInputMode.DegreesMinutesAndSeconds)
            {
                int dLat = (int)pointLatLng.Lat;
                double mLat = (pointLatLng.Lat - dLat) * 60;
                int miLat = (int)mLat;
                double sLat = (mLat - miLat) * 60;

                int dLng = (int)pointLatLng.Lng;
                double mLng = (pointLatLng.Lng - dLng) * 60;
                int miLng = (int)mLng;
                double sLng = (mLng - miLng) * 60;

                return $@"{dLat:00}{degreeSign} {miLat:00}{minuteSign} {sLat:00.00}{secondSign}   {dLng:00}{degreeSign} {miLng:00}{minuteSign} {sLng:00.00}{secondSign}";
            }
            return "";
        }

        public static double GetDistanceKm(this PointLatLng a, PointLatLng b)
        {
            const double r = 6371;
            var lat1 = DegreeToRadian(a.Lat);
            var lat2 = DegreeToRadian(b.Lat);
            var deltaLat = DegreeToRadian(b.Lat - a.Lat);
            var deltaLng = DegreeToRadian(b.Lng - a.Lng);

            var h = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
            return r * c;
        }

        private static double DegreeToRadian(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }


    }
}
