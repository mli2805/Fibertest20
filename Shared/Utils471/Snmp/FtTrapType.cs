namespace Utils471
{
    // should be the same as in MIB file
    public enum FtTrapType
    {
        MeasurementAsSnmp = 100,
        RtuNetworkEventAsSnmp = 200,
        BopNetworkEventAsSnmp = 300,
        RtuStatusEventAsSnmp = 400,
        TestTrap = 777,
    }
}