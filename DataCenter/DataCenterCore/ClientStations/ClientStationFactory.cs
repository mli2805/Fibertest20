using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public static class ClientStationFactory
    {
        public static ClientStation Create(RegisterClientDto dto, User user)
        {
            return new ClientStation()
            {
                UserId = user.UserId,
                UserName = dto.UserName,
                UserRole = user.Role,
                ClientIp = dto.Addresses.Main.GetAddress(),
                ClientAddressPort = dto.Addresses.Main.Port,
                ConnectionId = dto.ConnectionId,

                IsUnderSuperClient = dto.IsUnderSuperClient,
                IsWebClient = dto.IsWebClient,
                IsDesktopClient = !dto.IsUnderSuperClient && !dto.IsWebClient,

                LastConnectionTimestamp = DateTime.Now,
            };
        }
    }
}