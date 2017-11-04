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
                StationId = rtu.Id,
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
                StationId = dto.RtuId,
                Version = dto.Version,
                MainAddress = dto.PcDoubleAddress.Main.GetAddress(),
                MainAddressPort = dto.PcDoubleAddress.Main.Port,
                LastConnectionByMainAddressTimestamp = DateTime.Now,
                IsReserveAddressSet = dto.PcDoubleAddress.HasReserveAddress,
            };
            if (dto.PcDoubleAddress.HasReserveAddress)
            {
                rtuStation.ReserveAddress = dto.PcDoubleAddress.Reserve.GetAddress();
                rtuStation.ReserveAddressPort = dto.PcDoubleAddress.Reserve.Port;
                rtuStation.LastConnectionByReserveAddressTimestamp = DateTime.Now;
            }
            return rtuStation;
        }
    }
}