using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class ClientStationsRepository
    {
        private readonly ISettings _settings;
        private readonly IMyLog _logFile;
        private readonly IModel _writeModel;

        public ClientStationsRepository(ISettings settings, IMyLog logFile, IModel writeModel)
        {
            _settings = settings;
            _logFile = logFile;
            _writeModel = writeModel;
        }
      
        private async Task<ClientRegisteredDto> RegisterHeartbeat(RegisterClientDto dto)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
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
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    if (dbContext.ClientStations.Any(s => s.UserId == user.UserId && s.ClientGuid != dto.ClientId))
                    {
                        _logFile.AppendLine($"User {dto.UserName} registered on another PC");
                        result.ReturnCode = ReturnCode.ThisUserRegisteredOnAnotherPc;
                        return result;
                    }

                    var station = dbContext.ClientStations.FirstOrDefault(s => s.ClientGuid == dto.ClientId);
                    if (station != null)
                    {
                        station.UserId = user.UserId;
                        station.UserName = dto.UserName;
                        station.LastConnectionTimestamp = DateTime.Now;
                        await dbContext.SaveChangesAsync();
                        _logFile.AppendLine($"Station {dto.ClientId.First6()} was registered already. Re-registered.");
                    }
                    else
                    {
                        station = new ClientStation()
                        {
                            UserId = user.UserId,
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
                    result.UserId = user.UserId;
                    result.Role = user.Role;
                    var zone = _writeModel.Zones.First(z => z.ZoneId == user.ZoneId);
                    result.ZoneId = zone.ZoneId;
                    result.ZoneTitle = zone.Title;
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
                var user = _writeModel.Users.FirstOrDefault(u => u.Title == dto.UserName && UserExt.FlipFlop(u.EncodedPassword) == dto.Password);
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
                using (var dbContext = new FtDbContext(_settings.Options))
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
                using (var dbContext = new FtDbContext(_settings.Options))
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
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    DateTime noLaterThan = DateTime.Now - timeSpan;
                    var deadStations = dbContext.ClientStations.Where(s => s.LastConnectionTimestamp < noLaterThan).ToList();
                    foreach (var deadStation in deadStations)
                    {
                        _logFile.AppendLine($"Dead station {deadStation.ClientGuid.First6()} will be removed.");
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
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var clientStations = clientId == null 
                        ? await dbContext.ClientStations.ToListAsync() 
                        : await dbContext.ClientStations.Where(c => c.ClientGuid == clientId).ToListAsync();
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