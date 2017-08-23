﻿namespace Iit.Fibertest.UtilsLib
{
    public enum IniKey
    {
        // Server
        RtuGuid,
        HasReserveAddress,
        ServerTcpPort,
        Ip,
        Host,
        IsAddressIp,
        TcpPort,

        // General
        LocalIp,
        RtuServiceIp,
        Culture,
        LogFileSizeLimitKb,
        OtdrIp,
        OtdrPort,
        OtauIp,
        OtauPort,
        LogLevel,
        CheckNewMoniResultTimeout,
        CheckChannelsTimeout,

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