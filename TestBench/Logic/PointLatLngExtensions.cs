using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
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

                return $@"{dLat:#0}{degreeSign} {miLat:#0}{minuteSign} {sLat:#0.00}{secondSign}   {dLng:#0}{degreeSign} {miLng:#0}{minuteSign} {sLng:#0.00}{secondSign}";
            }
            return "";
        }


    }
}
