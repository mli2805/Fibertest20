using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class ClientRegistrationManager
    {
        private readonly IMyLog _logFile;
        //        private readonly IFibertestDbContext _dbContext;

        public ClientRegistrationManager(IMyLog logFile)
        {
            _logFile = logFile;
            //            _dbContext = dbContext;
        }

        private void SeedUsersTableIfNeeded()
        {
            var dbContext = new FtDbContext();
            if (dbContext.Users.Any())
                return; // seeded already

            var developer = new User() { Name = "developer", EncodedPassword = FlipFlop("developer"), Email = "", IsEmailActivated = false, Role = Role.Developer, IsDefaultZoneUser = true };
            dbContext.Users.Add(developer);
            var root = new User() { Name = "root", EncodedPassword = FlipFlop("root"), Email = "", IsEmailActivated = false, Role = Role.Root, IsDefaultZoneUser = true };
            dbContext.Users.Add(root);
            var oper = new User() { Name = "operator", EncodedPassword = FlipFlop("operator"), Email = "", IsEmailActivated = false, Role = Role.Operator, IsDefaultZoneUser = true };
            dbContext.Users.Add(oper);
            var supervisor = new User() { Name = "supervisor", EncodedPassword = FlipFlop("supervisor"), Email = "", IsEmailActivated = false, Role = Role.Supervisor, IsDefaultZoneUser = true };
            dbContext.Users.Add(supervisor);
            var superclient = new User() { Name = "superclient", EncodedPassword = FlipFlop("superclient"), Email = "", IsEmailActivated = false, Role = Role.Superclient, IsDefaultZoneUser = true };
            dbContext.Users.Add(superclient);
            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SeedUsersTableIfNeeded:" + e.Message);
            }
        }
        private string FlipFlop(string before)
        {
            return string.IsNullOrEmpty(before) ? "" : before.Substring(before.Length - 1, 1) + FlipFlop(before.Substring(0, before.Length - 1));
        }


        private Task<ClientRegisteredDto> CheckUserPassword(RegisterClientDto dto)
        {
            SeedUsersTableIfNeeded();
            var result = new ClientRegisteredDto();

            try
            {
                var dbContext = new FtDbContext();
                var users = dbContext.Users.ToList(); // there is no field Password in Db , so it should be instances in memory to address that property
                var user = users.FirstOrDefault(u => u.Name == dto.UserName && FlipFlop(u.EncodedPassword) == dto.Password);
                if (user != null)
                {
                    result.UserId = user.Id;
                    result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
                }
                else
                {
                    result.ReturnCode = ReturnCode.NoSuchUserOrWrongPassword;
                }
                return Task.FromResult(result);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                result.ReturnCode = ReturnCode.DbError;
                result.ExceptionMessage = e.Message;
                return Task.FromResult(result);
            }

        }

        private async Task<ClientRegisteredDto> RegisterHeartbeat(RegisterClientDto dto)
        {
            try
            {
                var dbContext = new FtDbContext();
                var station = dbContext.ClientStations.FirstOrDefault(s => s.ClientGuid == dto.ClientId);
                if (station != null)
                {
                    station.LastConnectionTimestamp = DateTime.Now;
                    await dbContext.SaveChangesAsync();
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientRegisteredSuccessfully };
                }
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.NoSuchClientStation };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterHeartbeat:" + e.Message);
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.DbError, ExceptionMessage = e.Message };
            }
        }
        private async Task<ClientRegisteredDto> RegisterClientStation(RegisterClientDto dto)
        {
            var result = new ClientRegisteredDto();
            try
            {
                var dbContext = new FtDbContext();
                var user = dbContext.Users.FirstOrDefault(u => u.Name == dto.UserName);
                if (user == null)
                {
                    result.ReturnCode = ReturnCode.NoSuchUserOrWrongPassword;
                    return result;
                }

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
                result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
                return result;
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


            var result = await CheckUserPassword(dto);
            if (result.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
                return result;
            return await RegisterClientStation(dto);
        }

        public async Task<int> UnregisterClientAsync(UnRegisterClientDto dto)
        {
            try
            {
                var dbContext = new FtDbContext();
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
            catch (Exception e)
            {
                _logFile.AppendLine("UnregisterClientAsync" + e.Message);
                return -1;
            }
        }

        public async Task<int> CleanClientStations()
        {
            try
            {
                var dbContext = new FtDbContext();
                dbContext.ClientStations.RemoveRange(dbContext.ClientStations);
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("CleanClientStations" + e.Message);
                return -1;
            }
        }

        public async Task<int> CleanDeadClients(TimeSpan timeSpan)
        {
            try
            {
                var dbContext = new FtDbContext();
                DateTime noLaterThan = DateTime.Now - timeSpan;
                var deadStations = dbContext.ClientStations.Where(s => s.LastConnectionTimestamp < noLaterThan).ToList();
                foreach (var deadStation in deadStations)
                {
                    _logFile.AppendLine($"Dead station {deadStation.ClientGuid} will be removed.");
                    dbContext.ClientStations.Remove(deadStation);
                }
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("CleanDeadClients:" + e.Message);
                return -1;
            }
        }

        private async Task<List<ClientStation>> GetAllLiveClients()
        {
            try
            {
                var dbContext = new FtDbContext();
                return await dbContext.ClientStations.ToListAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetAllLiveClients:" + e.Message);
                return null;
            }
        }

        public async Task<List<DoubleAddress>> GetClientsAddresses()
        {
            var allClients = await GetAllLiveClients();
            if (allClients == null || !allClients.Any())
                return null;

            return ExtractClientsAddresses(allClients);
        }

        private List<DoubleAddress> ExtractClientsAddresses(List<ClientStation> clientStations)
        {
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
}