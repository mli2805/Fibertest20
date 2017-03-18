using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class RequestAddEquipmentAtGpsLocation
    {
        public EquipmentType Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}