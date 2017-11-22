using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DatabaseLibrary
{
    public static class MapperToRtuStation
    {
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