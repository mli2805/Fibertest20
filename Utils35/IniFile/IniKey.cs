namespace Iit.Fibertest.Utils35
{
    public enum IniKey
    {
        ServerIp,
        ServerPort,
        CurrentMeasurementFile,
        BaseMeasurementFile,
        MeasurementWithBaseFile,

        // General
        RtuServiceIp,
        Culture,
        OtdrIp,
        OtdrPort,
        OtauIp,
        OtauPort,
        LogLevel,

        // Charon
        ConnectionTimeout,
        ReadTimeout,
        WriteTimeout,
        ComPort,
        ComSpeed,
        PauseAfterReset,

        // Monitoring
        IsMonitoringOn,
        PreciseMakeTimespan,
        PreciseSaveTimespan,
        FastSaveTimespan,

        // Restore
        ResetCharon,
        ClearArp,
        RestartService,
        RebootSystem
    }
}