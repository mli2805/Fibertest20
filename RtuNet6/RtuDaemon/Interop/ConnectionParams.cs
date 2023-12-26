// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.RtuDaemon;

public struct ConnectionParams
{
    public float reflectance; // R -dB
    public float splice; // dB (better say "loss")
    public float snr_almax;
}