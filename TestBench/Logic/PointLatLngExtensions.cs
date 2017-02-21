using System.Globalization;
using GMap.NET;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench.Properties;

namespace Iit.Fibertest.TestBench
{
    public static class PointLatLngExtensions
    {
        public static string ToStringInDegrees(this PointLatLng point)
        {
            return string.Format(CultureInfo.CurrentCulture, Resources.SID_coor_in_degrees, point.Lat, point.Lng);
        }
    }
}
