using System;
using GMap.NET;

namespace Iit.Fibertest.Graph.Algorithms
{
    public static class GpsCalculator
    {
        public static int GpsInSorFormat(double degrees)
        {
            int d = (int)degrees;
            double m = (degrees - d) * 60;
            int mi = (int)m;
            double s = (m - mi) * 60;
            int ss = (int)(s * 100);
            if (s * 100 - ss >= 0.5) ss++;
            return d * 1000000 + mi * 10000 + ss;
        }

        public static double GetDistanceBetweenPointLatLng(PointLatLng p1, PointLatLng p2)
        {
            return GetDistanceBetweenPointsInDegrees(p1.Lat, p1.Lng, p2.Lat, p2.Lng);
        }
        // in meters
        public static double GetDistanceBetweenPointsInDegrees(
            double lat1Degree, double lng1Degree, double lat2Degree, double lng2Degree)
        {

            return GetDistanceBetweenPointsInRadians(Degree2Radian(lat1Degree), Degree2Radian(lng1Degree), 
                Degree2Radian(lat2Degree), Degree2Radian(lng2Degree));
        }

        private static double Degree2Radian(double degree)
        {
            return degree * Math.PI / 180;
        }

        // in meters
        private static double GetDistanceBetweenPointsInRadians(
            double lat1Radian, double lng1Radian, double lat2Radian, double lng2Radian)
        {
            const int earthRadius = 6372795;

            var lat1Cos = Math.Cos(lat1Radian);
            var lat2Cos = Math.Cos(lat2Radian);
            var lat1Sin = Math.Sin(lat1Radian);
            var lat2Sin = Math.Sin(lat2Radian);

            var sinDeltaLong = Math.Sin(lng2Radian - lng1Radian);
            var cosDeltaLong = Math.Cos(lng2Radian - lng1Radian);

            var y = Math.Sqrt(Math.Pow(lat2Cos * sinDeltaLong, 2) + Math.Pow(lat1Cos * lat2Sin - lat1Sin * lat2Cos * cosDeltaLong, 2));
            var x = lat1Sin * lat2Sin + lat1Cos * lat2Cos * cosDeltaLong;

            return Math.Atan2(y, x) * earthRadius;
        }
    }
}