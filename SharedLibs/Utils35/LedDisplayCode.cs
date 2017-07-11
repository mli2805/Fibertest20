namespace Iit.Fibertest.Utils35
{
    public enum LedDisplayCode : byte
    {
        Wait = 0x13,
        Connecting = 0x14,

        ErrorConnectOtdr = 0x82,
        ErrorConnectOtau = 0x83,
        ErrorConnectBop = 0x84,
        ErrorTogglePort = 0x85,
    }
}