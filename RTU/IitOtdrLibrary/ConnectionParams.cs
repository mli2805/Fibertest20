// ReSharper disable InconsistentNaming

using System.Runtime.InteropServices;

namespace Iit.Fibertest.IitOtdrLibrary
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConnectionParams
    {
        public float reflectance; // R -dB
        public float splice; // dB (better say "loss")
        public float snr_almax;
    }
}
