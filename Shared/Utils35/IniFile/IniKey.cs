namespace Iit.Fibertest.UtilsLib
{
    public enum IniKey
    {
        // Server
        ClientGuidOnServer,
        IsInGisVisibleMode,
        RtuGuid,
        HasReserveAddress,
        ServerTitle,
        Ip,
        Host,
        IsAddressIp,
        TcpPort,

        // General
        Version,
        Culture,
        LogFileSizeLimitKb,
        LogLevel,

        // General - Client
        ClientPollingRateMs,
        FailedPollsLimit,
        // General - RTU
        RtuHeartbeatRate,
        // General - DataCenter
        CheckHeartbeatEvery,
        ClientConnectionsPermittedGap,
        RtuHeartbeatPermittedGap,

        EventSourcingPortion,

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
        Kadastr,

      
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
        ResetDb,

        //Broadcast
        GsmModemComPort,
        EventLifetimeLimit,
        TestNumberToSms,
        TestSmsContent,

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
        MaxZoom,
        GMapProvider,

        // Miscellaneous
        PathToSor,
        MaxCableReserve,
        GpsInputMode,
        GraphVisibilityLevel,
        IsGraphVisibleOnStart,
        PathToClient,
    }
}