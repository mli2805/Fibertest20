using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public static class RtuStationFactory
    {
        public static RtuStation Create(RtuInitializedDto dto)
        {
            var rtuStation = new RtuStation()
            {
                RtuGuid = dto.RtuId,
                Version = dto.Version,
                MainAddress = dto.RtuAddresses.Main.GetAddress(),
                MainAddressPort = dto.RtuAddresses.Main.Port,
                LastConnectionByMainAddressTimestamp = DateTime.Now,
                IsMainAddressOkDuePreviousCheck = true,
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