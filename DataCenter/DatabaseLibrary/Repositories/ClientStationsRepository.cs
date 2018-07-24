using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly Model _writeModel;

        public ClientStationsRepository(ISettings settings, IMyLog logFile, Model writeModel )
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

        private ClientRegisteredDto ExceededNumber()
        {
            _logFile.AppendLine("Exceeded the number of clients registered simultaneously");
            return new ClientRegisteredDto(){ReturnCode = ReturnCode.ExceededNumberOfClients};
        }

        private ClientRegisteredDto TheSameUser(string username)
        {
            _logFile.AppendLine($"User {username} registered on another PC");
            return new ClientRegisteredDto() { ReturnCode = ReturnCode.ThisUserRegisteredOnAnotherPc };
        }

        private async Task ReRegister(FtDbContext dbContext, RegisterClientDto dto, ClientStation station, User user)
        {
            station.UserId = user.UserId;
            station.UserName = dto.UserName;
            station.LastConnectionTimestamp = DateTime.Now;
            await dbContext.SaveChangesAsync();
            _logFile.AppendLine($"Station {dto.ClientId.First6()} was registered already. Re-registered.");
        }

        private async Task RegisterNew(FtDbContext dbContext, RegisterClientDto dto, User user)
        {
            var station = new ClientStation()
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

        private ClientRegisteredDto FillInSuccessfulResult(User user)
        {
            var result = new ClientRegisteredDto();
            result.UserId = user.UserId;
            result.Role = user.Role;
            var zone = _writeModel.Zones.First(z => z.ZoneId == user.ZoneId);
            result.ZoneId = zone.ZoneId;
            result.ZoneTitle = zone.Title;
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            result.DatacenterVersion = fvi.FileVersion;
            result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
            return result;
        }

        private async Task<ClientRegisteredDto> RegisterClientStation(RegisterClientDto dto, User user)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    if (dbContext.ClientStations.Count() >= _writeModel.License.ClientStationCount)
                        return ExceededNumber();
                    if (dbContext.ClientStations.Any(s => s.UserId == user.UserId && s.ClientGuid != dto.ClientId))
                        return TheSameUser(dto.UserName);
                  
                    var station = dbContext.ClientStations.FirstOrDefault(s => s.ClientGuid == dto.ClientId);
                    if (station != null)
                        await ReRegister(dbContext, dto, station, user);
                    else
                        await RegisterNew(dbContext, dto, user);

                    _logFile.AppendLine($"There are {dbContext.ClientStations.Count()} client(s)");
                    return FillInSuccessfulResult(user);
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterClientStation:" + e.Message);
                return new ClientRegisteredDto
                {
                    ReturnCode = ReturnCode.DbError,
                    ExceptionMessage = e.Message
                };
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

        public async Task<List<ClientStation>> CleanDeadClients(TimeSpan timeSpan)
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
                    await dbContext.SaveChangesAsync();
                    return deadStations;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("CleanDeadClients:" + e.Message);
                return null;
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