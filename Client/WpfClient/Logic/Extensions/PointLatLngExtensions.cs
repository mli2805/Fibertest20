using System;
using GMap.NET;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class PointLatLngExtensions
    {
        public static string ToDetailedString(this PointLatLng pointLatLng, GpsInputMode mode)
        {
            string degreeSign = @"°";
            string minuteSign = @"′";
            string secondSign = @"″";
            if (mode == GpsInputMode.Degrees)
            {
                return $@"{pointLatLng.Lat:00.000000}{degreeSign}   {pointLatLng.Lng:00.000000}{degreeSign}";
            }
            if (mode == GpsInputMode.DegreesAndMinutes)
            {
                int dLat = (int)pointLatLng.Lat;
                double mLat = (pointLatLng.Lat - dLat) * 60;

                int dLng = (int)pointLatLng.Lng;
                double mLng = (pointLatLng.Lng - dLng) * 60;

                return $@"{dLat:00}{degreeSign} {mLat:00.0000}{minuteSign}    {dLng:00}{degreeSign} {mLng:00.0000}{minuteSign}";
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

                return $@"{dLat:00}{degreeSign} {miLat:00}{minuteSign} {sLat:00.00}{secondSign}     {dLng:00}{degreeSign} {miLng:00}{minuteSign} {sLng:00.00}{secondSign}";
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
