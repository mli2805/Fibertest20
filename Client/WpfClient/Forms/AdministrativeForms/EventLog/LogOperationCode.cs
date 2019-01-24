namespace Iit.Fibertest.Client
{
    public enum LogOperationCode
    {
        ClientStarted = 101,
        ClientExited = 102,
        ClientConnectionLost = 103,

        RtuAdded = 201,
        RtuUpdated,
        RtuInitialized,
        RtuRemoved,

        TraceAdded = 301,
        TraceUpdated,
        TraceAttached,
        TraceDetached,
        TraceCleaned,
        TraceRemoved,

        BaseRefAssigned = 401,
        MonitoringSettingsChanged,
        MonitoringStarted,
        MonitoringStopped,

        MeasurementUpdated,
    }
}