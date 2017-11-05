using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DatabaseLibrary
{
    public static class MapperToRtuStation
    {
        public static RtuStation Map(Rtu rtu)
        {
            var rtuStation = new RtuStation()
            {
                RtuGuid = rtu.Id,
                Version = rtu.Version,
                MainAddress = rtu.MainChannel.GetAddress(),
                MainAddressPort = rtu.MainChannel.Port,
                LastConnectionByMainAddressTimestamp = DateTime.Now,
                IsReserveAddressSet = rtu.IsReserveChannelSet,
            };
            if (rtu.IsReserveChannelSet)
            {
                rtuStation.ReserveAddress = rtu.ReserveChannel.GetAddress();
                rtuStation.ReserveAddressPort = rtu.ReserveChannel.Port;
                rtuStation.LastConnectionByReserveAddressTimestamp = DateTime.Now;
            }
            return rtuStation;
        }

        public static RtuStation Map(RtuInitializedDto dto)
        {
            var rtuStation = new RtuStation()
            {
                RtuGuid = dto.RtuId,
                Version = dto.Version,
                MainAddress = dto.RtuAddresses.Main.GetAddress(),
                MainAddressPort = dto.RtuAddresses.Main.Port,
                LastConnectionByMainAddressTimestamp = DateTime.Now,
                IsReserveAddressSet = dto.RtuAddresses.HasReserveAddress,
            };
            if (dto.RtuAddresses.HasReserveAddress)
            {
                rtuStation.ReserveAddress = dto.RtuAddresses.Reserve.GetAddress();
                rtuStation.ReserveAddressPort = dto.RtuAddresses.Reserve.Port;
                rtuStation.LastConnectionByReserveAddressTimestamp = DateTime.Now;
            }
            return rtuStation;
        }
    }
}