namespace Utils471
{
    // should be the same as in MIB file
    public enum FtTrapProperty
    {
        EventId = 0,
        EventRegistrationTime,
        RtuTitle,
        TraceTitle,

        RtuMainChannel = 10,
        RtuReserveChannel = 11,

        BopTitle = 20,
        BopState,

        TraceState = 30,
        AccidentNodeTitle,
        AccidentType,
        AccidentToRtuDistanceKm,
        AccidentGps,

        LeftNodeTitle = 40,
        LeftNodeGps,
        LeftNodeToRtuDistanceKm,

        RightNodeTitle = 50,
        RightNodeGps,
        RightNodeToRtuDistanceKm,

        BaseRefType = 60,
        RtuStatusEventType = 61,
        RtuStatusEventName = 62,

        TestString = 700,
        TestInt,
        TestDouble,
    }
}