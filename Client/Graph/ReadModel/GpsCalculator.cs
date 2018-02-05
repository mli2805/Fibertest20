using System;

namespace Iit.Fibertest.Graph
{
    public static class GpsCalculator
    {
        // in meters
        public static double CalculateGpsDistanceBetweenPointsInDegrees(
            double lat1Degree, double lng1Degree, double lat2Degree, double lng2Degree)
        {

            return CalculateGpsDistanceBetweenPointInRadians(Degree2Radian(lat1Degree), Degree2Radian(lng1Degree), 
                Degree2Radian(lat2Degree), Degree2Radian(lng2Degree));
        }

        private static double Degree2Radian(double degree)
        {
            return degree * Math.PI / 180;
        }

        // in meters
        private static double CalculateGpsDistanceBetweenPointInRadians(
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