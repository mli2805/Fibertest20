using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class ClientStationsRepository
    {
        private readonly IMyLog _logFile;

        public ClientStationsRepository(IMyLog logFile)
        {
            _logFile = logFile;
        }

        private async Task<bool> SeedUsers(FtDbContext dbContext)
        {
                try
                {
                    dbContext.Users.Add(new User() { Name = "developer", EncodedPassword = FlipFlop("developer"), Email = "", IsEmailActivated = false, Role = Role.Developer, IsDefaultZoneUser = true });
                    dbContext.Users.Add(new User() { Name = "root", EncodedPassword = FlipFlop("root"), Email = "", IsEmailActivated = false, Role = Role.Root, IsDefaultZoneUser = true });
                    dbContext.Users.Add(new User() { Name = "operator", EncodedPassword = FlipFlop("operator"), Email = "", IsEmailActivated = false, Role = Role.Operator, IsDefaultZoneUser = true });
                    dbContext.Users.Add(new User() { Name = "supervisor", EncodedPassword = FlipFlop("supervisor"), Email = "", IsEmailActivated = false, Role = Role.Supervisor, IsDefaultZoneUser = true });
                    dbContext.Users.Add(new User() { Name = "superclient", EncodedPassword = FlipFlop("superclient"), Email = "", IsEmailActivated = false, Role = Role.Superclient, IsDefaultZoneUser = true });
                    await dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception e)
                {
                    _logFile.AppendLine("SeedUsers:" + e.Message);
                    return false;
                }
        }
        private string FlipFlop(string before)
        {
            return string.IsNullOrEmpty(before) ? "" : before.Substring(before.Length - 1, 1) + FlipFlop(before.Substring(0, before.Length - 1));
        }

        private async Task<User> CheckUserPasswordAsync(RegisterClientDto dto)
        {
            using (var dbContext = new FtDbContext())
            {
                try
                {
                    if (!dbContext.Users.Any() &&
                        await SeedUsers(dbContext) == false)
                            return null;

                    var users = await dbContext.Users.ToListAsync(); // there is no field Password in Db , so it should be instances in memory to address that property
                    return users.FirstOrDefault(u => u.Name == dto.UserName && FlipFlop(u.EncodedPassword) == dto.Password);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine("CheckUserPasswordAsync:" + e.Message);
                    return null;
                }                
            }
        }

        private async Task<ClientRegisteredDto> RegisterHeartbeat(RegisterClientDto dto)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    var station = dbContext.ClientStations.FirstOrDefault(s => s.ClientGuid == dto.ClientId);
                    if (station != null)
                    {
                        station.LastConnectionTimestamp = DateTime.Now;
                        await dbContext.SaveChangesAsync();
                        return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientRegisteredSuccessfully };
                    }
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.NoSuchClientStation };
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterHeartbeat:" + e.Message);
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.DbError, ExceptionMessage = e.Message };
            }
        }

        private async Task<ClientRegisteredDto> RegisterClientStation(RegisterClientDto dto, User user)
        {
            var result = new ClientRegisteredDto();
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    if (dbContext.ClientStations.Any(s => s.UserId == user.Id && s.ClientGuid != dto.ClientId))
                    {
                        _logFile.AppendLine($"User {dto.UserName} registered on another PC");
                        result.ReturnCode = ReturnCode.ThisUserRegisteredOnAnotherPc;
                        return result;
                    }

                    var station = dbContext.ClientStations.FirstOrDefault(s => s.ClientGuid == dto.ClientId);
                    if (station != null)
                    {
                        station.UserId = user.Id;
                        station.UserName = dto.UserName;
                        station.LastConnectionTimestamp = DateTime.Now;
                        await dbContext.SaveChangesAsync();
                        _logFile.AppendLine($"Station {dto.ClientId.First6()} was registered already. Re-registered.");
                    }
                    else
                    {
                        station = new ClientStation()
                        {
                            UserId = user.Id,
                            UserName = dto.UserName,
                            ClientGuid = dto.ClientId,
                            ClientAddress = dto.Addresses.Main.GetAddress(),
                            ClientAddressPort = dto.Addresses.Main.Port,
                            LastConnectionTimestamp = DateTime.Now,
                        };
                        dbContext.ClientStations.Add(station);
                        await dbContext.SaveChangesAsync();
                        if (!dto.IsHeartbeat)
                            _logFile.AppendLine($"Client station {dto.ClientId.First6()} registered");
                    }

                    _logFile.AppendLine($"There are {dbContext.ClientStations.Count()} client(s)");
                    result.UserId = user.Id;
                    result.Role = user.Role;
                    result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
                    return result;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterClientStation:" + e.Message);
                result.ReturnCode = ReturnCode.DbError;
                result.ExceptionMessage = e.Message;
                return result;
            }
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            if (dto.IsHeartbeat)
                return await RegisterHeartbeat(dto);

            var result = new ClientRegisteredDto();
            try
            {
                var user = await CheckUserPasswordAsync(dto);
                if (user == null)
                {
                    result.ReturnCode = ReturnCode.NoSuchUserOrWrongPassword;
                    return result;
                }
                return await RegisterClientStation(dto, user);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterClientStation:" + e.Message);
                result.ReturnCode = ReturnCode.DbError;
                result.ExceptionMessage = e.Message;
                return result;
            }
        }

        public async Task<int> UnregisterClientAsync(UnRegisterClientDto dto)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    var station = dbContext.ClientStations.FirstOrDefault(s => s.ClientGuid == dto.ClientId);
                    if (station == null)
                    {
                        _logFile.AppendLine("There is no client station with such guid");
                        return 0;
                    }

                    dbContext.ClientStations.Remove(station);
                    var countAffected = await dbContext.SaveChangesAsync();
                    _logFile.AppendLine($"Client unregistered. There are {dbContext.ClientStations.Count()} client(s) now");
                    return countAffected;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("UnregisterClientAsync" + e.Message);
                return -1;
            }
        }

        public async Task<int> CleanClientStationsTable()
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    dbContext.ClientStations.RemoveRange(dbContext.ClientStations);
                    return await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("CleanClientStationsTable" + e.Message);
                return -1;
            }
        }

        public async Task<int> CleanDeadClients(TimeSpan timeSpan)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    DateTime noLaterThan = DateTime.Now - timeSpan;
                    var deadStations = dbContext.ClientStations.Where(s => s.LastConnectionTimestamp < noLaterThan).ToList();
                    foreach (var deadStation in deadStations)
                    {
                        _logFile.AppendLine($"Dead station {deadStation.ClientGuid} will be removed.");
                        dbContext.ClientStations.Remove(deadStation);
                    }
                    return await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("CleanDeadClients:" + e.Message);
                return -1;
            }
        }


        public async Task<List<DoubleAddress>> GetClientsAddresses(Guid? clientId = null)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    var clientStations = await dbContext.ClientStations.Where(c => c.ClientGuid == clientId).ToListAsync();
                    if (clientStations == null || !clientStations.Any())
                        return null;

                    var result = new List<DoubleAddress>();
                    foreach (var clientStation in clientStations)
                    {
                        result.Add(new DoubleAddress()
                        {
                            Main = new NetAddress(clientStation.ClientAddress, clientStation.ClientAddressPort)
                        });
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetClientsAddresses:" + e.Message);
                return null;
            }
        }
    }
}