using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public static class ClientsRegistrator
    {
        // R1
        public static ClientRegisteredDto CheckLicense(this ClientsCollection collection, RegisterClientDto dto)
        {
            if (collection.WriteModel.Licenses.Count == 0)
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.NoLicenseHasBeenAppliedYet };

            if (dto.IsUnderSuperClient)
            {
                if (collection.Clients.Count(c => c.UserRole == Role.Superclient) >= collection.WriteModel.GetSuperClientStationLicenseCount()
                    && collection.Clients.All(s => s.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.SuperClientsCountExceeded };
            }
            else if (dto.IsWebClient)
            {
                if (collection.Clients.Count(c => c.IsWebClient) >= collection.WriteModel.GetWebClientLicenseCount()
                    && collection.Clients.All(s => s.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.WebClientsCountExceeded };
            }
            else
            {
                if (collection.Clients.Count(c => c.IsDesktopClient) >= collection.WriteModel.GetClientStationLicenseCount()
                    && collection.Clients.All(s => s.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientsCountExceeded };
            }
            return null;
        }

        // R2 Check user and password

        // R3
        public static ClientRegisteredDto CheckRights(this User user, RegisterClientDto dto)
        {
            if (dto.IsUnderSuperClient)
            {
                if (!user.Role.IsSuperclientPermitted())
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.UserHasNoRightsToStartSuperClient };
            }
            else if (dto.IsWebClient)
            {
                if (!user.Role.IsWebPermitted())
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.UserHasNoRightsToStartWebClient };
            }
            else
            {
                if (!user.Role.IsDesktopPermitted())
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.UserHasNoRightsToStartClient };
            }
            return null;
        }


        // R4
        public static async Task<ClientRegisteredDto> CheckTheSameUser(
            this ClientsCollection collection, RegisterClientDto dto, User user)
        {
            var stationWithTheSameUser = collection.Clients.FirstOrDefault(s => s.UserId == user.UserId);
            if (stationWithTheSameUser != null)
            {
                // both clients are desktop
                if (!dto.IsWebClient && !stationWithTheSameUser.IsWebClient)
                {
                    collection.LogFile.AppendLine($"The same user {dto.UserName} registered from device {stationWithTheSameUser.ClientIp}");
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.ThisUserRegisteredFromAnotherDevice };
                }
                else
                // different types of clients or both clients are web
                {
                    collection.LogFile.AppendLine($"The same client {stationWithTheSameUser.UserName}/{stationWithTheSameUser.ClientIp} with connectionId {stationWithTheSameUser.ConnectionId} removed.");
                    // notify old station
                    var serverAsksClientToExitDto = new ServerAsksClientToExitDto()
                    {
                        ToAll = false,
                        ConnectionId = stationWithTheSameUser.ConnectionId,
                        Reason = UnRegisterReason.UserRegistersAnotherSession,

                        IsNewUserWeb = dto.IsWebClient,
                        NewAddress = dto.ClientIp,
                    };
                    await collection.D2CWcfService.ServerAsksClientToExit(serverAsksClientToExitDto);
                    await collection.FtSignalRClient.NotifyAll("ServerAsksClientToExit", serverAsksClientToExitDto.ToCamelCaseJson());
                    await Task.Delay(1000);
                    collection.Clients.Remove(stationWithTheSameUser);
                    collection.LogFile.AppendLine("Old client deleted");
                }
            }
            return null;
        }


        // R5
        public static ClientRegisteredDto CheckMachineKey(this ClientsCollection collection, RegisterClientDto dto, User user)
        {
            if (!collection.WriteModel.IsMachineKeyRequired()) return null;
            if (user.MachineKey == dto.MachineKey) return null;

            if (string.IsNullOrEmpty(dto.SecurityAdminPassword))
            {
                // prohibited, call Security Admin to confirm 
                return user.MachineKey == null
                    ? new ClientRegisteredDto() { ReturnCode = ReturnCode.EmptyMachineKey }
                    : new ClientRegisteredDto() { ReturnCode = ReturnCode.WrongMachineKey };
            }

            var admin = collection.WriteModel.Users.First(u => u.Role == Role.SecurityAdmin);
            if (admin.EncodedPassword != dto.SecurityAdminPassword)
            {
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.WrongSecurityAdminPassword };
            }

            // if SecurityAdminPassword is sent correctly or it is a first connection for user
            user.MachineKey = dto.MachineKey;

            return null;
        }

        public static ClientRegisteredDto FillInSuccessfulResult(this ClientsCollection collection, RegisterClientDto dto, User user)
        {
            var result = new ClientRegisteredDto();
            result.UserId = user.UserId;
            result.Role = user.Role;
            var zone = collection.WriteModel.Zones.First(z => z.ZoneId == user.ZoneId);
            result.ZoneId = zone.ZoneId;
            result.ZoneTitle = zone.Title;
            result.ConnectionId = dto.ConnectionId;
            result.DatacenterVersion = collection.CurrentDatacenterParameters.DatacenterVersion;
            result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
            result.IsWithoutMapMode = collection.IniFile.Read(IniSection.Server, IniKey.IsWithoutMapMode, false);
            result.Smtp = collection.CurrentDatacenterParameters.Smtp;
            result.GsmModemComPort = collection.CurrentDatacenterParameters.GsmModemComPort;
            result.Snmp = collection.CurrentDatacenterParameters.Snmp;
            return result;
        }
    }
}