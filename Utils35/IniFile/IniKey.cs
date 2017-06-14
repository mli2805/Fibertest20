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
        Culture,
        OtdrIp,
        OtdrPort,
        OtauIp,
        OtauPort,

        // Charon
        ConnectionTimeout,
        ReadTimeout,
        WriteTimeout,
        LogLevel,
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