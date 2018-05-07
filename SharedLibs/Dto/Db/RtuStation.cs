using System;

namespace Iit.Fibertest.Dto
{
    public class RtuStation
    {
        public int Id { get; set; }
        public Guid RtuGuid { get; set; }
        public string Version { get; set; }

        public string MainAddress { get; set; }
        public int MainAddressPort { get; set; }
        public DateTime LastConnectionByMainAddressTimestamp { get; set; }
        public bool IsMainAddressOkDuePreviousCheck { get; set; }

        public bool IsReserveAddressSet { get; set; }
        public string ReserveAddress { get; set; }
        public int ReserveAddressPort { get; set; }
        public DateTime LastConnectionByReserveAddressTimestamp { get; set; }
        public bool IsReserveAddressOkDuePreviousCheck { get; set; }

    }

    public static class RtuStationExt
    {
        public static DoubleAddress GetRtuDoubleAddress(this RtuStation rtuStation)
        {
            var rtuAddresses = new DoubleAddress()
            {
                Main = GetAddress(rtuStation.MainAddress, rtuStation.MainAddressPort),
                HasReserveAddress = rtuStation.IsReserveAddressSet,
            };
            if (rtuAddresses.HasReserveAddress)
                rtuAddresses.Reserve = GetAddress(rtuStation.ReserveAddress, rtuStation.ReserveAddressPort);
            return rtuAddresses;
        }

        private static NetAddress GetAddress(string address, int port)
        {
            var netAddress = new NetAddress();
            if (NetAddress.IsValidIp4(address))
            {
                netAddress.Ip4Address = address;
                netAddress.IsAddressSetAsIp = true;
            }
            else
            {
                netAddress.HostName = address;
                netAddress.IsAddressSetAsIp = false;
            }
            netAddress.Port = port;
            return netAddress;
        }
    }
}