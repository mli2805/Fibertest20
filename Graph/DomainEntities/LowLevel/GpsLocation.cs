namespace Iit.Fibertest.Graph
{
    public class GpsLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public GpsLocation()
        {
        }

        public GpsLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public string ToString(GpsInputMode mode)
        {
            string degreeSign = "\u00B0";
            string minuteSign = "\u2032";
            string secondSign = "\u2033";
            if (mode == GpsInputMode.Degrees)
            {
                return $@"{Latitude:#0.000000}{degreeSign} {Longitude:#0.000000}{degreeSign}";
            }
            if (mode == GpsInputMode.DegreesAndMinutes)
            {
                int dLat = (int)Latitude;
                double mLat = (Latitude - dLat) * 60;

                int dLng = (int)Longitude;
                double mLng = (Longitude - dLng) * 60;

                return $@"{dLat:#0}{degreeSign} {mLat:#0.0000}{minuteSign}  {dLng:#0}{degreeSign} {mLng:#0.0000}{minuteSign}";
            }
            if (mode == GpsInputMode.DegreesMinutesAndSeconds)
            {
                int dLat = (int)Latitude;
                double mLat = (Latitude - dLat) * 60;
                int miLat = (int)mLat;
                double sLat = (mLat - miLat) * 60;

                int dLng = (int)Longitude;
                double mLng = (Longitude - dLng) * 60;
                int miLng = (int)mLng;
                double sLng = (mLng - miLng) * 60;

                return $@"{dLat:#0}{degreeSign} {miLat:#0}{minuteSign} {sLat:#0.00}{secondSign}   {dLng:#0}{degreeSign} {miLng:#0}{minuteSign} {sLng:#0.00}{secondSign}";
            }
            return "";
        }

    }
}
