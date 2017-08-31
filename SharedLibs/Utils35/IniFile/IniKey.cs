namespace Iit.Fibertest.UtilsLib
{
    public enum IniKey
    {
        // Server
        ClientGuidOnServer,
        RtuGuid,
        HasReserveAddress,
        ServerTcpPort,
        Ip,
        Host,
        IsAddressIp,
        TcpPort,

        // General
        LocalIp,
        Culture,
        LogFileSizeLimitKb,
        OtdrIp,
        OtdrPort,
        OtauIp,
        OtauPort,
        LogLevel,
        CheckChannelsTimeout,
        CheckRtuIsAliveTimeout,
        PermittedTimeBetweenConnection,

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
        OpenTimeoutMs,
        ReceiveTimeout,
        SendTimeout,
        PingTimeoutMs,
    }
}