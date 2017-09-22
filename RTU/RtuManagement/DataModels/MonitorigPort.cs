using System;
using System.IO;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.UtilsLib;

namespace RtuManagement
{
    public class MonitorigPort
    {
        public NetAddress NetAddress { get; set; }
        public bool IsPortOnMainCharon { get; set; }
        public int OpticalPort { get; set; }

        public DateTime? LastPreciseMadeTimestamp { get; set; }
        public DateTime LastPreciseSavedTimestamp { get; set; }
        public DateTime LastFastSavedTimestamp { get; set; }

        public FiberState LastTraceState { get; set; }
        public MoniResult LastMoniResult { get; set; }
        public bool IsBreakdownCloserThen20Km { get; set; }

        public MonitorigPort(NetAddress netAddress, int opticalOpticalPort, FiberState lastTraceState)
        {
            NetAddress = netAddress;
            OpticalPort = opticalOpticalPort;
            LastTraceState = lastTraceState;

            LastFastSavedTimestamp = DateTime.Now;
            LastPreciseSavedTimestamp = DateTime.Now;
        }

        public MonitorigPort(OtauPortDto port)
        {
            NetAddress = new NetAddress(port.OtauIp, port.OtauTcpPort);
            OpticalPort = port.OpticalPort;
            LastTraceState = FiberState.NotChecked;

            LastFastSavedTimestamp = DateTime.Now;
            LastPreciseSavedTimestamp = DateTime.Now;
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

        public string ToStringA()
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
            var basefile = $@"..\PortData\{GetFolderName()}\{BaseRefType.Additional.ToBaseFileName()}";
            return File.Exists(basefile);
        }

        public byte[] GetBaseBytes(BaseRefType baseRefType, IMyLog rtuLog)
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appDir = Path.GetDirectoryName(appPath);
            var basefile = appDir + $@"\..\PortData\{GetFolderName()}\{baseRefType.ToBaseFileName()}";
            if (File.Exists(basefile))
                return File.ReadAllBytes(basefile);
            rtuLog.AppendLine($"Can't find {basefile}");
            return null;
        }

        public void SaveMeasBytes(BaseRefType baseRefType, byte[] bytes)
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appDir = Path.GetDirectoryName(appPath);
            var measfile = appDir + $@"\..\PortData\{GetFolderName()}\{baseRefType.ToMeasFileName()}";
            File.WriteAllBytes(measfile, bytes);
        }
    }
}
