using System;
using System.IO;
using Dto;

namespace Iit.Fibertest.UtilsLib
{
    public class ExtendedPort
    {
        public NetAddress NetAddress { get; set; }
        public bool IsPortOnMainCharon { get; set; }
        public int OpticalPort { get; set; }

        public DateTime LastPreciseMadeTimestamp { get; set; }
        public DateTime LastPreciseSavedTimestamp { get; set; }
        public DateTime LastFastSavedTimestamp { get; set; }

        public MoniResult LastMoniResult { get; set; }
        public bool IsBreakdownCloserThen20Km { get; set; }

        public ExtendedPort(NetAddress netAddress, int opticalOpticalPort)
        {
            NetAddress = netAddress;
            OpticalPort = opticalOpticalPort;
        }

        public string GetFolderName()
        {
            return $"{NetAddress.Ip4Address}t{NetAddress.Port}p{OpticalPort}";
        }

        public string ToStringA()
        {
            return IsPortOnMainCharon
                ? $"{OpticalPort}"
                : $"{OpticalPort} on {NetAddress.ToStringA()}";
        }

        public bool HasAdditionalBase()
        {
            var basefile = $@"..\PortData\{GetFolderName()}\{BaseRefType.Additional.ToBaseFileName()}";
            return File.Exists(basefile);
        }

        public byte[] GetBaseBytes(BaseRefType baseRefType, LogFile rtuLog)
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

        public static ExtendedPort Create(string str, LogFile rtuLog)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            var parts = str.Split('-');
            if (parts.Length != 2)
            {
                rtuLog.AppendLine($"Invalid string in queue file: '{str}'", 2);
                return null;
            }

            int opticalPort;
            if (!int.TryParse(parts[1], out opticalPort))
            {
                rtuLog.AppendLine($"Can't parse optical port: '{parts[1]}'", 2);
                return null;
            }

            var addressParts = parts[0].Split(':');
            if (addressParts.Length != 2)
            {
                rtuLog.AppendLine($"Can't parse address: '{parts[0]}'", 2);
                return null;
            }

            int tcpPort;
            if (!int.TryParse(addressParts[1], out tcpPort))
            {
                rtuLog.AppendLine($"Can't parse tcp port: '{addressParts[1]}'", 2);
                return null;
            }

            var netAddress = new NetAddress(addressParts[0], tcpPort);
            if (!netAddress.HasValidIp4Address() || !netAddress.HasValidTcpPort())
            {
                rtuLog.AppendLine($"Invalid ip address: '{parts[0]}'", 2);
                return null;
            }

            return new ExtendedPort(netAddress, opticalPort);
        }
    }
}
