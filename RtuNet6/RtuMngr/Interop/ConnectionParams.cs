// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.RtuMngr;

public struct ConnectionParams
{
    public float reflectance; // R -dB
    public float splice; // dB (better say "loss")
    public float snr_almax;
}