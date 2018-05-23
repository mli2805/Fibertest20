namespace Iit.Fibertest.UtilsLib
{
    public enum IniKey
    {
        // Server
        ClientGuidOnServer,
        RtuGuid,
        HasReserveAddress,
        Ip,
        Host,
        IsAddressIp,
        TcpPort,

        // General
        Version,
        Culture,
        LogFileSizeLimitKb,
        LogLevel,

        // RtuManager
        OtdrIp,
        OtdrPort,
        OtauIp,
        OtauPort,
        PreviousOwnPortCount,
        ShouldLogHeartbeatProblems,

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
        LastMeasurementTimestamp,

        // Recovering
        MikrotikRebootTimeout,
        MikrotikRebootAttemptsBeforeNotification,
        RecoveryStep,
        RebootSystemEnabled,
        RebootSystemDelay,


        // Watchdog
        RtuServiceName,
        LastRestartTime,
        MaxGapBetweenMeasurements,

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
        IsGraphVisibleOnStart,
    }
}