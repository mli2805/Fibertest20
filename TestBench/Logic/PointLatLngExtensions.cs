using System.Globalization;
using GMap.NET;

namespace Iit.Fibertest.TestBench
{
    public static class PointLatLngExtensions
    {
        public static string ToStringInDegrees(this PointLatLng point)
        {
            return string.Format(CultureInfo.CurrentCulture, "{{Lat={0:#0.000000}, Lng={1:#0.000000}}}", point.Lat, point.Lng);
        }
    }
}
