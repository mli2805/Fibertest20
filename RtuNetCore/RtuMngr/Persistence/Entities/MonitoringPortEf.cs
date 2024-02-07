using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuMngr;

public class MonitoringPortEf
{
    public int Id { get; init; }

    public bool IsPortOnMainCharon { get; set; }

    public string CharonSerial { get; set; } = null!;
    public int OpticalPort{ get; set; }
    public Guid TraceId{ get; set; }

    public DateTime LastPreciseMadeTimestamp{ get; set; }
    public DateTime LastPreciseSavedTimestamp{ get; set; }
    public DateTime LastFastMadeTimestamp{ get; set; }
    public DateTime LastFastSavedTimestamp{ get; set; }

    public FiberState LastTraceState{ get; set; }

    public string? LastMoniResult { get; set; }

    public bool IsBreakdownCloserThen20Km{ get; set; }

    public bool IsMonitoringModeChanged{ get; set; }
    public bool IsConfirmationRequired{ get; set; }
}
