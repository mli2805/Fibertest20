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
}