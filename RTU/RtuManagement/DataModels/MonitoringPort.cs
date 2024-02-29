using System;
using System.IO;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public class MonitoringPort
    {
        public bool IsPortOnMainCharon { get; set; }

        public string CharonSerial { get; set; }
        public int OpticalPort { get; set; }
        public Guid TraceId { get; set; }

        public DateTime LastPreciseMadeTimestamp { get; set; }
        public DateTime LastPreciseSavedTimestamp { get; set; }
        public DateTime LastFastSavedTimestamp { get; set; }

        public FiberState LastTraceState { get; set; }
        public MoniResult LastMoniResult { get; set; }
        public bool IsBreakdownCloserThen20Km { get; set; }

        public bool IsMonitoringModeChanged { get; set; }
        public bool IsConfirmationRequired { get; set; }

        public MonitoringPort(MonitoringPortOnDisk port)
        {
            CharonSerial = port.Serial;
            OpticalPort = port.OpticalPort;
            TraceId = port.TraceId;
            IsPortOnMainCharon = port.IsPortOnMainCharon;
            LastTraceState = port.LastTraceState;

            LastPreciseMadeTimestamp = port.LastPreciseMadeTimestamp ?? DateTime.Now;
            LastFastSavedTimestamp = port.LastFastSavedTimestamp;
            LastPreciseSavedTimestamp = port.LastPreciseSavedTimestamp;

            IsMonitoringModeChanged = port.IsMonitoringModeChanged;
            IsConfirmationRequired = port.IsConfirmationRequired;

            if (port.LastMoniResult != null)
                LastMoniResult = new MoniResult()
                {
                    HardwareReturnCode = port.LastMoniResult.HardwareReturnCode,
                    UserReturnCode = port.LastMoniResult.UserReturnCode,
                    IsNoFiber = port.LastMoniResult.IsNoFiber,
                    IsFiberBreak = port.LastMoniResult.IsFiberBreak,
                    Levels = port.LastMoniResult.Levels,
                    BaseRefType = port.LastMoniResult.BaseRefType,
                    FirstBreakDistance = port.LastMoniResult.FirstBreakDistance,
                    Accidents = port.LastMoniResult.Accidents
                };
        }

        // new port for monitoring in user's command
        public MonitoringPort(PortWithTraceDto port)
        {
            CharonSerial = port.OtauPort.Serial;
            OpticalPort = port.OtauPort.OpticalPort;
            IsPortOnMainCharon = port.OtauPort.IsPortOnMainCharon;
            TraceId = port.TraceId;
            LastTraceState = port.LastTraceState;

            LastPreciseMadeTimestamp = DateTime.Now;
            LastFastSavedTimestamp = DateTime.Now;
            LastPreciseSavedTimestamp = DateTime.Now;

            LastMoniResult = new MoniResult()
            {
                // user sends ReturnCode.MeasurementEndedNormally if there is no accidents on this trace
                UserReturnCode = port.LastRtuAccidentOnTrace, 
                HardwareReturnCode = ReturnCode.MeasurementEndedNormally
            };
            IsMonitoringModeChanged = true;
        }

        private string GetPortFolderName()
        {
            return $"{CharonSerial}p{OpticalPort:000}";
        }

        private string ToStringA()
        {
            return IsPortOnMainCharon
                ? $"{OpticalPort}"
                : $"{OpticalPort} on {CharonSerial}";
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
            var basefile = AppDomain.CurrentDomain.BaseDirectory + $@"..\PortData\{GetPortFolderName()}\{BaseRefType.Additional.ToBaseFileName()}";
            return File.Exists(basefile);
        }

        public byte[] GetBaseBytes(BaseRefType baseRefType, IMyLog rtuLog)
        {
            var basefile = AppDomain.CurrentDomain.BaseDirectory + $@"..\PortData\{GetPortFolderName()}\{baseRefType.ToBaseFileName()}";
            if (File.Exists(basefile))
                return File.ReadAllBytes(basefile);
            rtuLog.AppendLine($"Can't find {basefile}");
            return null;
        }

        public void SaveSorData(BaseRefType baseRefType, byte[] bytes, SorType sorType, IMyLog rtuLog)
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Measurements\");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var filename = Path.Combine(folder, $@"{DateTime.Now:ddMM HHmmss} {baseRefType} {sorType}.sor");

            try
            {
                File.WriteAllBytes(filename, bytes);
            }
            catch (Exception e)
            {
                rtuLog.AppendLine($"Failed to persist measurement data into {filename}");
                rtuLog.AppendLine(e.Message);
            }
        }

        public void SaveMeasBytes(BaseRefType baseRefType, byte[] bytes, SorType sorType, IMyLog rtuLog)
        {
            var measfile = AppDomain.CurrentDomain.BaseDirectory +
                           $@"..\PortData\{GetPortFolderName()}\{baseRefType.ToFileName(sorType)}";

            try
            {
                if (baseRefType == BaseRefType.Precise && sorType == SorType.Meas && File.Exists(measfile))
                {
                    var previousFile = AppDomain.CurrentDomain.BaseDirectory
                                       + $@"..\PortData\{GetPortFolderName()}\{baseRefType.ToFileName(SorType.Previous)}";
                    if (File.Exists(previousFile))
                        File.Delete(previousFile);
                    File.Move(measfile, previousFile);
                }
                File.WriteAllBytes(measfile, bytes);
            }
            catch (Exception e)
            {
                rtuLog.AppendLine($"Failed to persist measurement data into {measfile}");
                rtuLog.AppendLine(e.Message);
            }
        }
    }
}
