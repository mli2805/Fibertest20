namespace Iit.Fibertest.UtilsLib
{
    public enum IniKey
    {
        // Server
        IsWithoutMapMode,
        RtuGuid,
        HasReserveAddress,
        ServerTitle,
        Ip,
        Host,
        IsAddressIp,
        PreviousStartOnVersion,
     //   TcpPort,
        ClientOrdinal, // will be added to to the TCP port number 11843; for Client is 0, but under SuperClient would be different

        // General
        Version,
        Culture,
        LogFileSizeLimitKb,
        LogLevel,

        // General - Client
        ClientPollingRateMs,
        ClientHeartbeatRateMs,
        FailedPollsLimit,
        Ip4Default,
        // General - RTU
        RtuHeartbeatRate,
        RtuPauseAfterReboot,
        RtuUpTimeForAdditionalPause,
        // General - DataCenter
        AskVeexRtuEvery,
        CheckOutOfTurnRequests,
        CheckCompletedTestsEvery,
        CheckWebApiEvery,
        CheckHeartbeatEvery,
        ClientConnectionsPermittedGap,
        RtuHeartbeatPermittedGap,
        // General - WebApi
        NudgeSignalrTimeout,

        EventSourcingPortion,

        // RtuManager
        OtdrIp,
        OtdrPort,
        OtauIp,
        OtauPort,
        PreviousOwnPortCount,
        ShouldLogHeartbeatProblems,

        //DirectCharonLibrary
        BopIpAddress,

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
        IsAutoBaseMeasurementInProgress,
        PreciseMakeTimespan,
        PreciseSaveTimespan,
        FastSaveTimespan,
        LastMeasurementTimestamp,
        LastAutoBaseMeasurementTimestamp,
        SaveSorData,
        KeepOtdrConnection,

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
        MaxGapBetweenAutoBaseMeasurements,

        // NetTcpBinding
        OpenTimeoutMs,
        ReceiveTimeout,
        SendTimeout,
        PingTimeoutMs,

        //MySql
        MySqlTcpPort,
        MySqlDbSchemePostfix,
        ResetDb,
        FreeSpaceThresholdGb,
        IsOptimizationCouldBeDoneUpToToday,

        //Broadcast
        GsmModemComPort,
        EventLifetimeLimit,
        TestNumberToSms,
        TestSmsContent,
        MsmqTestRtuId,
        MsmqTestTraceId,

        // Smtp
        SmtpHost,
        SmtpPort,
        MailFrom,
        MailFromPassword,
        SmtpTimeoutMs,

        // Snmp,
        IsSnmpOn,
        SnmpTrapVersion,
        SnmpReceiverIp,
        SnmpReceiverPort,
        SnmpAgentIp,
        SnmpCommunity,
        EnterpriseOid,
        SnmpEncoding,

        // Map
        Zoom,
        CenterLatitude,
        CenterLongitude,
        MaxZoom,
        SaveMaxZoomNoMoreThan,
        GMapProvider,
        MapAccessMode,
        IsHighDensityGraph,
        ThresholdZoom,
        ScreenPartAsMargin,

        // Miscellaneous
        PathToSor,
        MaxCableReserve,
        GpsInputMode,
        GraphVisibilityLevel,
        PathToClient,
        PathToRftsParamsTemplate,
        MeasurementTimeoutMs,
        DoNotSignalAboutSuspicion,
        VeexLineParamsTimeoutMs,

        // LoadTesting
        Multiplier,
        Pause,

        // WebApi
        DomainName,
        BindingProtocol,

        // Uninstall
        IsOnRtu,

        // OtdrParameters
        OpUnit,
        OpDistance,
        OpResolution,
        OpPulseDuration,
        OpMeasurementTime,

        // RtuEmulator
        RtuId,
        TraceId,
        MainCharonPort,
        BopCharonPort,
    }
}