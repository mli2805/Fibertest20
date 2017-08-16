using System;
using System.IO;
using Dto;

namespace Iit.Fibertest.Utils35
{
    public enum PortMeasResult
    {
        Unknown            =0, // just started , no previous measurements
        Ok                 =1,
        BrokenByFast       =2,
        BrokenByPrecise    =3,
    }
    public class ExtendedPort
    {
        public NetAddress NetAddress { get; set; }
        public bool IsPortOnMainCharon { get; set; }
        public int Port { get; set; }

        public DateTime LastPreciseMadeTimestamp { get; set; }
        public DateTime LastPreciseSavedTimestamp { get; set; }
        public DateTime LastFastSavedTimestamp { get; set; }

        public PortMeasResult State { get; set; }
        public bool IsBreakdownCloserThen20Km { get; set; }

        public ExtendedPort(NetAddress netAddress, int opticalPort)
        {
            NetAddress = netAddress;
            Port = opticalPort;
        }

        public string GetFolderName()
        {
            return $"{NetAddress.Ip4Address}t{NetAddress.Port}p{Port}";
        }

        public string ToStringA()
        {
            return $"{Port} on {NetAddress.ToStringA()}";
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
