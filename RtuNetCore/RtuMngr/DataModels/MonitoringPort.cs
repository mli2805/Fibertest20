using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public class MonitoringPort
{
    public int Id { get; init; }

    public bool IsPortOnMainCharon;

    public string CharonSerial = string.Empty;
    public int OpticalPort;
    public Guid TraceId;

    public DateTime LastPreciseMadeTimestamp = DateTime.MinValue;
    public DateTime LastPreciseSavedTimestamp;
    public DateTime LastFastMadeTimestamp = DateTime.MinValue;
    public DateTime LastFastSavedTimestamp;

    public FiberState LastTraceState;
    public MoniResult? LastMoniResult;
    public bool IsBreakdownCloserThen20Km;

    public bool IsMonitoringModeChanged;
    public bool IsConfirmationRequired;

    public DateTime LastMadeTimestamp =>
        LastFastMadeTimestamp > LastPreciseMadeTimestamp ? LastFastMadeTimestamp : LastPreciseMadeTimestamp;

    public MonitoringPort() { }

    // new port for monitoring in user's command
    public MonitoringPort(PortWithTraceDto port)
    {
        CharonSerial = port.OtauPort.Serial ?? "";
        OpticalPort = port.OtauPort.OpticalPort;
        IsPortOnMainCharon = port.OtauPort.IsPortOnMainCharon;
        TraceId = port.TraceId;
        LastTraceState = port.LastTraceState;

        LastFastSavedTimestamp = DateTime.Now;
        LastPreciseSavedTimestamp = DateTime.Now;

        IsMonitoringModeChanged = true;
    }

    private string ToStringA()
    {
        return IsPortOnMainCharon
            ? $"{OpticalPort}"
            : $"{OpticalPort} on {CharonSerial}";
    }

    private string GetThisPortDataFolder()
    {
        var fibertestPath = FileOperations.GetMainFolder();
        return Path.Combine(fibertestPath, $@"portdata/{CharonSerial}p{OpticalPort:000}");
    }

    public string ToStringB(Charon mainCharon)
    {
        if (CharonSerial == mainCharon.Serial)
            return OpticalPort.ToString();
        foreach (var pair in mainCharon.Children)
        {
            if (pair.Value.Serial == CharonSerial)
                return $"{pair.Key}:{OpticalPort}";
        }
        return $"Can't find port {ToStringA()}";
    }

    public bool HasAdditionalBase()
    {
        return File.Exists($@"{GetThisPortDataFolder()}/{BaseRefType.Additional.ToBaseFileName()}");
    }

    public byte[]? GetBaseBytes<T>(BaseRefType baseRefType, ILogger<T> logger)
    {
        var baseFile = $@"{GetThisPortDataFolder()}/{baseRefType.ToBaseFileName()}";
        if (File.Exists(baseFile))
            return File.ReadAllBytes(baseFile);
        logger.Error(Logs.RtuManager, $"Can't find {baseFile}");
        return null;
    }

    public void SaveSorData<T>(BaseRefType baseRefType, byte[] bytes, SorType sorType, ILogger<T> logger)
    {
        var fibertestPath = FileOperations.GetMainFolder();
        var folder = Path.Combine(fibertestPath, @"Measurements");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        var filename = Path.Combine(folder, $@"{DateTime.Now:ddMM HHmmss} {baseRefType} {sorType}.sor");

        try
        {
            File.WriteAllBytes(filename, bytes);
        }
        catch (Exception e)
        {
            logger.Error(Logs.RtuManager, $"Failed to persist measurement data into {filename}");
            logger.Error(Logs.RtuManager, e.Message);
        }
    }

    public void SaveMeasBytes<T>(BaseRefType baseRefType, byte[] bytes, SorType sorType, ILogger<T> logger)
    {
        var measFile = $@"{GetThisPortDataFolder()}/{baseRefType.ToFileName(sorType)}";

        try
        {
            if (baseRefType == BaseRefType.Precise && sorType == SorType.Meas && File.Exists(measFile))
            {
                var previousFile = $@"{GetThisPortDataFolder()}/{baseRefType.ToFileName(SorType.Previous)}";
                if (File.Exists(previousFile))
                    File.Delete(previousFile);
                File.Move(measFile, previousFile);
            }
            File.WriteAllBytes(measFile, bytes);
        }
        catch (Exception e)
        {
            logger.Error(Logs.RtuManager, $"Failed to persist measurement data into {measFile}");
            logger.Error(Logs.RtuManager, e.Message);
        }
    }

    public void SetMadeTimeStamp(BaseRefType baseType)
    {
        if (baseType == BaseRefType.Fast)
            LastFastMadeTimestamp = DateTime.Now;
        else
            LastPreciseMadeTimestamp = DateTime.Now;
    }
    
    public void SetSavedTimeStamp(BaseRefType baseType)
    {
        if (baseType == BaseRefType.Fast)
            LastFastSavedTimestamp = DateTime.Now;
        else
            LastPreciseSavedTimestamp = DateTime.Now;
    }
}