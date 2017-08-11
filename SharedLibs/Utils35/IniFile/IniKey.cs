namespace Iit.Fibertest.Utils35
{
    public enum IniKey
    {
        // DataCenter
        MainAddress,
        RtuGuid,
        HasReserveAddress,
        ServerTcpPort,
        ReserveAddress,
        CurrentMeasurementFile,
        BaseMeasurementFile,
        MeasurementWithBaseFile,

        // General
        LocalIp,
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

        // Recovering
        MikrotikRebootTimeout,
        RecoveryStep,
        RebootSystemEnabled,
        RebootSystemDelay,

        // Watchdog
        RtuServiceName,

        // NetTcpBinding
        OpenTimeout,
        ReceiveTimeout,
        SendTimeout,
        PingTimeout,
    }
}