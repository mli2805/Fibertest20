using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuMngr;

public class MonitoringPortEf
{
    public int Id { get; init; }

    public bool IsPortOnMainCharon;

    public string CharonSerial { get; set; } = null!;
    public int OpticalPort{ get; set; }
    public Guid TraceId{ get; set; }

    public DateTime LastPreciseMadeTimestamp{ get; set; }
    public DateTime LastPreciseSavedTimestamp{ get; set; }
    public DateTime LastFastSavedTimestamp{ get; set; }

    public FiberState LastTraceState{ get; set; }

    public MoniResultEf LastMoniResult { get; set; } = null!;

    public bool IsBreakdownCloserThen20Km{ get; set; }

    public bool IsMonitoringModeChanged{ get; set; }
    public bool IsConfirmationRequired{ get; set; }
}

public class MoniResultEf
{
    public int Id { get; init; }

    public ReturnCode UserReturnCode { get; set; }
    public ReturnCode HardwareReturnCode { get; set; }

    public bool IsNoFiber { get; set; }
    public bool IsFiberBreak { get; set; }
    public List<MoniLevelEf> Levels { get; set; } = new List<MoniLevelEf>();

    public BaseRefType BaseRefType { get; set; }
    public double FirstBreakDistance { get; set; }

    public List<AccidentInSorEf> Accidents { get; set; } = new List<AccidentInSorEf>();
}

public class MoniLevelEf
{
    public int Id { get; init; }

    public bool IsLevelFailed { get; set; }
    public MoniLevelType Type { get; set; }
}

public class AccidentInSorEf
{
    public int Id { get; init; }

    public int BrokenRftsEventNumber { get; set; }

    public FiberState AccidentSeriousness { get; set; }
    public OpticalAccidentType OpticalTypeOfAccident { get; set; }

    public bool IsAccidentInOldEvent { get; set; }
    public double AccidentToRtuOpticalDistanceKm { get; set; }

    public string EventCode { get; set; } = null!;
    public double DeltaLen { get; set; }
}