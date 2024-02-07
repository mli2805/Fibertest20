using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuMngr;

public static class MappingEf
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };


    public static MonitoringPort FromEf(this MonitoringPortEf port)
    {
        var result = new MonitoringPort()
        {
            Id = port.Id,
            CharonSerial = port.CharonSerial,
            OpticalPort = port.OpticalPort,
            IsPortOnMainCharon = port.IsPortOnMainCharon,
            TraceId = port.TraceId,
            LastTraceState = port.LastTraceState,

            LastFastMadeTimestamp = port.LastFastMadeTimestamp,
            LastPreciseMadeTimestamp = port.LastPreciseMadeTimestamp,
            LastFastSavedTimestamp = port.LastFastSavedTimestamp,
            LastPreciseSavedTimestamp = port.LastPreciseSavedTimestamp,

            IsBreakdownCloserThen20Km = port.IsBreakdownCloserThen20Km,
            IsMonitoringModeChanged = port.IsMonitoringModeChanged,
            IsConfirmationRequired = port.IsConfirmationRequired,
        };

        if (port.LastMoniResult != null)
        {
            var lastMoniResult = JsonConvert.DeserializeObject<MoniResult>(port.LastMoniResult, JsonSerializerSettings);
            result.LastMoniResult = lastMoniResult;

        }
        return result;

    }

    public static MonitoringPortEf ToEf(this MonitoringPort port)
    {
        var result = new MonitoringPortEf()
        {
            Id = port.Id,
            CharonSerial = port.CharonSerial,
            OpticalPort = port.OpticalPort,
            TraceId = port.TraceId,
            IsPortOnMainCharon = port.IsPortOnMainCharon,
            LastTraceState = port.LastTraceState,

            LastFastMadeTimestamp = port.LastFastMadeTimestamp,
            LastPreciseMadeTimestamp = port.LastPreciseMadeTimestamp,
            LastFastSavedTimestamp = port.LastFastSavedTimestamp,
            LastPreciseSavedTimestamp = port.LastPreciseSavedTimestamp,

            IsBreakdownCloserThen20Km = port.IsBreakdownCloserThen20Km,
            IsMonitoringModeChanged = port.IsMonitoringModeChanged,
            IsConfirmationRequired = port.IsConfirmationRequired,
        };

        if (port.LastMoniResult != null)
        {
            // omit SorBytes property !!!
            var newMoniResult = new MoniResult()
            {
                UserReturnCode = port.LastMoniResult.UserReturnCode,
                HardwareReturnCode = port.LastMoniResult.HardwareReturnCode,
                IsNoFiber = port.LastMoniResult.IsNoFiber,
                IsFiberBreak = port.LastMoniResult.IsFiberBreak,
                Levels = port.LastMoniResult.Levels,
                BaseRefType = port.LastMoniResult.BaseRefType,
                FirstBreakDistance = port.LastMoniResult.FirstBreakDistance,
                Accidents = port.LastMoniResult.Accidents,
            };

            var json = JsonConvert.SerializeObject(newMoniResult, JsonSerializerSettings);
            result.LastMoniResult = json;
        }

        return result;
    }

    public static MonitoringResultDto FromEf(this MonitoringResultEf moniResult)
    {
        var dto = new MonitoringResultDto()
        {
            ReturnCode = moniResult.ReturnCode,
            Reason = moniResult.Reason,
            RtuId = moniResult.RtuId,
            TimeStamp = moniResult.TimeStamp,
            TraceState = moniResult.TraceState,
            PortWithTrace = new PortWithTraceDto()
            {
                OtauPort = new OtauPortDto()
                {
                    Serial = moniResult.Serial,
                    IsPortOnMainCharon = moniResult.IsPortOnMainCharon,
                    OpticalPort = moniResult.OpticalPort,
                },
                TraceId = moniResult.TraceId,
                LastTraceState = moniResult.TraceState,
            },
            BaseRefType = moniResult.BaseRefType,
            SorBytes = moniResult.SorBytes
        };

        return dto;

    }

    public static MonitoringResultEf ToEf(this MoniResult moniResult, MonitoringPort monitoringPort,
        Guid rtuId, ReasonToSendMonitoringResult reason)
    {
        var dto = new MonitoringResultEf()
        {
            ReturnCode = moniResult.UserReturnCode,
            Reason = reason,
            RtuId = rtuId,
            TimeStamp = DateTime.Now,
            Serial = monitoringPort.CharonSerial,
            IsPortOnMainCharon = monitoringPort.IsPortOnMainCharon,
            OpticalPort = monitoringPort.OpticalPort,
            TraceId = monitoringPort.TraceId,
            BaseRefType = moniResult.BaseRefType,
            TraceState = moniResult.GetAggregatedResult(),
            SorBytes = moniResult.SorBytes
        };
        return dto;
    }
}