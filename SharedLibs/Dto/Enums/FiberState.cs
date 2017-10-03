namespace Iit.Fibertest.Dto
{
    public enum FiberState
    {
        //
        NotInTrace = 0x0,
        NotJoined = 0x1,
        //
        NotChecked = 0x2,
        //
        Ok = 0x4,
        Suspect = 0x5,
        Minor = 0x6,
        Major = 0x7,
        Critical = 0x8,
        User = 0x9,
        FiberBreak = 0xA,
        NoFiber = 0xB,
        //
        HighLighted = 0xE,
        DistanceMeasurement = 0xF,
    }
}