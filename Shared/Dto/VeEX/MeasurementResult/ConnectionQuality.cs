
// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.Dto
{
    public class ConnectionQuality
    {
        public string laserUnit { get; set; }
        public double lmaxKm { get; set; }
        public double lmaxNs { get; set; }
        public double loss { get; set; }
        public double reflectance { get; set; }
        public double snr { get; set; }
        public double vscoutSetsCount { get; set; }
    }
}
