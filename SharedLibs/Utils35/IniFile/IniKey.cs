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
        Version,
        LocalIp,
        Culture,
        LogFileSizeLimitKb,
        OtdrIp,
        OtdrPort,
        OtauIp,
        OtauPort,
        LogLevel,

        // Migrator
        OldFibertestServerIp,  
        ShouldTransferMeasurements,

        ClientHeartbeatRate,
        ClientPollingRateMs,
        RtuHeartbeatRate,

        CheckHeartbeatEvery,
        ClientHeartbeatPermittedGap,
        RtuHeartbeatPermittedGap,

        EventSourcingPortion,

        // Charon
        ConnectionTimeout,
        ReadTimeout,
        WriteTimeout,
        ComPort,
        ComSpeed,
        PauseAfterReset,
        PauseBetweenCommandsMs,

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

        //MySql
        MySqlTcpPort,
        MySqlDbSchemePostfix,

        // Smtp
        SmtpHost,
        SmtpPort,
        MailFrom,
        MailFromPassword,
        SmtpTimeoutMs,

        // Map
        Zoom,
        CenterLatitude,
        CenterLongitude,

        // Miscellaneous
        PathToSor,
        MaxCableReserve,
        GpsInputMode,
        GraphVisibilityLevel,
    }
}