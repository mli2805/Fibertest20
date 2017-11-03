using System;
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
        private readonly IFibertestDbContext _dbContext;

        public ClientRegistrationManager(IMyLog logFile, IFibertestDbContext dbContext)
        {
            _logFile = logFile;
            _dbContext = dbContext;
        }

        private Task<ClientRegisteredDto> CheckUserPassword(RegisterClientDto dto)
        {
            var result = new ClientRegisteredDto();

            try
            {
                var users = _dbContext.Users.ToList();
                if (users.FirstOrDefault(u => u.Name == dto.UserName && u.Password == dto.Password) == null)
                {
                    result.ReturnCode = ReturnCode.NoSuchUserOrWrongPassword;
                    return Task.FromResult(result);
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                result.ReturnCode = ReturnCode.DbError;
                result.ExceptionMessage = e.Message;
                return Task.FromResult(result);
            }

            result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
            return Task.FromResult(result);
        }

        private async Task<ClientRegisteredDto> RegisterClientStation(RegisterClientDto dto)
        {
            var result = new ClientRegisteredDto();
            try
            {
                if (_dbContext.ClientStations.FirstOrDefault(s => s.Username == dto.UserName
                                                       && s.StationId != dto.ClientId) != null)
                {
                    _logFile.AppendLine("This user registered on another PC");
                    result.ReturnCode = ReturnCode.ThisUserRegisteredOnAnotherPc;
                    return result;
                }
                var station = _dbContext.ClientStations.FirstOrDefault(s => s.StationId == dto.ClientId);
                if (station != null)
                {
                    station.Username = dto.UserName;
                    station.StationIp = dto.Addresses.Main.Ip4Address;
                    station.LastConnectionTimestamp = DateTime.Now;
                    await _dbContext.SaveChangesAsync();
                    _logFile.AppendLine("This station was registered already. Re-registered.");
                }
                else
                {
                    station = new ClientStation()
                    {
                        Username = dto.UserName,
                        StationId = dto.ClientId,
                        StationIp = dto.Addresses.Main.Ip4Address,
                        LastConnectionTimestamp = DateTime.Now,
                    };
                    _dbContext.ClientStations.Add(station);
                    await _dbContext.SaveChangesAsync();
                    _logFile.AppendLine("Station registered");
                }

                _logFile.AppendLine($"There are {_dbContext.ClientStations.Count()} clients");
                result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                result.ReturnCode = ReturnCode.DbError;
                result.ExceptionMessage = e.Message;
                return result;
            }
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var result = await CheckUserPassword(dto);
            if (result.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
                return result;

            return await RegisterClientStation(dto);
        }

        public async Task<int> UnregisterClientAsync(UnRegisterClientDto dto)
        {
            try
            {
                var station = _dbContext.ClientStations.FirstOrDefault(s => s.StationId == dto.ClientId);
                if (station == null)
                {
                    _logFile.AppendLine("There is no client station with such guid");
                    return 0;
                }

                _dbContext.ClientStations.Remove(station);
                _logFile.AppendLine($"Client unregistered. There are {_dbContext.ClientStations.Count()} clients now");
                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return -1;
            }
        }

        public async Task<int> CleanClientStations()
        {
            try
            {
                _dbContext.ClientStations.RemoveRange(_dbContext.ClientStations);
                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return -1;
            }
        }

        //TODO 
//        public async Task<int> CleanDeadClients(TimeSpan timeSpan) { }
    }
}