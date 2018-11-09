using System;
using System.IO;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public class MonitorigPort
    {
        public NetAddress NetAddress { get; set; }
        public bool IsPortOnMainCharon { get; set; }
        public int OpticalPort { get; set; }
        public Guid TraceId { get; set; }

        public DateTime? LastPreciseMadeTimestamp { get; set; }
        public DateTime LastPreciseSavedTimestamp { get; set; }
        public DateTime LastFastSavedTimestamp { get; set; }

        public FiberState LastTraceState { get; set; }
        public MoniResult LastMoniResult { get; set; }
        public bool IsBreakdownCloserThen20Km { get; set; }

        public bool MonitoringModeChangedFlag { get; set; }

        public MonitorigPort(MonitoringPortOnDisk port)
        {
            NetAddress = port.NetAddress;
            OpticalPort = port.OpticalPort;
            TraceId = port.TraceId;
            LastTraceState = port.LastTraceState;
            IsPortOnMainCharon = port.IsPortOnMainCharon;

            LastFastSavedTimestamp = port.LastFastSavedTimestamp;
            LastPreciseSavedTimestamp = port.LastPreciseSavedTimestamp;

            MonitoringModeChangedFlag = port.MonitoringModeChangedFlag;
        }

        // new port for monitoring in user's command
        public MonitorigPort(PortWithTraceDto port)
        {
            NetAddress = new NetAddress(port.OtauPort.OtauIp, port.OtauPort.OtauTcpPort);
            OpticalPort = port.OtauPort.OpticalPort;
            IsPortOnMainCharon = port.OtauPort.IsPortOnMainCharon;
            TraceId = port.TraceId;
            LastTraceState = FiberState.Ok;

            LastFastSavedTimestamp = DateTime.Now;
            LastPreciseSavedTimestamp = DateTime.Now;

            MonitoringModeChangedFlag = true;
        }

        public bool IsTheSamePort(OtauPortDto otauPortDto)
        {
            return NetAddress.Equals(new NetAddress(otauPortDto.OtauIp, otauPortDto.OtauTcpPort))
                   && OpticalPort == otauPortDto.OpticalPort;
        }

        private string GetFolderName()
        {
            return $"{NetAddress.Ip4Address}t{NetAddress.Port}p{OpticalPort}";
        }

        private string ToStringA()
        {
            return IsPortOnMainCharon
                ? $"{OpticalPort}"
                : $"{OpticalPort} on {NetAddress.ToStringA()}";
        }

        public string ToStringB(Charon mainCharon)
        {
            if (NetAddress.Equals(mainCharon.NetAddress))
                return OpticalPort.ToString();
            foreach (var pair in mainCharon.Children)
            {
                if (pair.Value.NetAddress.Equals(NetAddress))
                    return $"{pair.Key}:{OpticalPort}";
            }
            return $"Can't find port {ToStringA()}";
        }

        public bool HasAdditionalBase()
        {
            var basefile = AppDomain.CurrentDomain.BaseDirectory + $@"..\PortData\{GetFolderName()}\{BaseRefType.Additional.ToBaseFileName()}";
            return File.Exists(basefile);
        }

        public byte[] GetBaseBytes(BaseRefType baseRefType, IMyLog rtuLog)
        {
            var basefile = AppDomain.CurrentDomain.BaseDirectory + $@"..\PortData\{GetFolderName()}\{baseRefType.ToBaseFileName()}";
            if (File.Exists(basefile))
                return File.ReadAllBytes(basefile);
            rtuLog.AppendLine($"Can't find {basefile}");
            return null;
        }

        public void SaveMeasBytes(BaseRefType baseRefType, byte[] bytes, bool isError = false)
        {
            var measfile = AppDomain.CurrentDomain.BaseDirectory + $@"..\PortData\{GetFolderName()}\{baseRefType.ToMeasFileName()}";
            if (isError)
                measfile = measfile + $"_error_{DateTime.Now}";
            File.WriteAllBytes(measfile, bytes);
        }
    }
}
