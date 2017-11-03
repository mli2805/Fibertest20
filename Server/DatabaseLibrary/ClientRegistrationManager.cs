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

        private void SeedUsersTableIfNeeded()
        {
            if (_dbContext.Users.Any())
                return; // seeded already

            var developer = new User() { Name = "developer", Password = "developer", Email = "", IsEmailActivated = false, Role = Role.Developer, IsDefaultZoneUser = true };
            _dbContext.Users.Add(developer);
            var root = new User() { Name = "root", Password = "root", Email = "", IsEmailActivated = false, Role = Role.Root, IsDefaultZoneUser = true };
            _dbContext.Users.Add(root);
            var oper = new User() { Name = "operator", Password = "operator", Email = "", IsEmailActivated = false, Role = Role.Operator, IsDefaultZoneUser = true };
            _dbContext.Users.Add(oper);
            var supervisor = new User() { Name = "supervisor", Password = "supervisor", Email = "", IsEmailActivated = false, Role = Role.Supervisor, IsDefaultZoneUser = true };
            _dbContext.Users.Add(supervisor);
            var superclient = new User() { Name = "superclient", Password = "superclient", Email = "", IsEmailActivated = false, Role = Role.Superclient, IsDefaultZoneUser = true };
            _dbContext.Users.Add(superclient);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SeedUsersTableIfNeeded:" + e.Message);
            }
        }

        private Task<ClientRegisteredDto> CheckUserPassword(RegisterClientDto dto)
        {
            SeedUsersTableIfNeeded();
            var result = new ClientRegisteredDto();

            try
            {
                var users = _dbContext.Users.ToList(); // there is no field Password in Db , so it should be instances in memory to address that property
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
                if (!dto.IsHeartbeat && // this check for registration, not for heartbeat
                    _dbContext.ClientStations.FirstOrDefault
                      (s => s.Username == dto.UserName && s.StationId != dto.ClientId) != null)
                {
                    _logFile.AppendLine("This user registered on another PC");
                    result.ReturnCode = ReturnCode.ThisUserRegisteredOnAnotherPc;
                    return result;
                }

                var station = _dbContext.ClientStations.FirstOrDefault(s => s.StationId == dto.ClientId);
                if (station != null)
                {
                    station.Username = dto.UserName;
                    station.LastConnectionTimestamp = DateTime.Now;
                    await _dbContext.SaveChangesAsync();
                    if (!dto.IsHeartbeat)
                        _logFile.AppendLine("This station was registered already. Re-registered.");
                }
                else
                {
                    station = new ClientStation()
                    {
                        Username = dto.UserName,
                        StationId = dto.ClientId,
                        LastConnectionTimestamp = DateTime.Now,
                    };
                    _dbContext.ClientStations.Add(station);
                    await _dbContext.SaveChangesAsync();
                    if (!dto.IsHeartbeat)
                        _logFile.AppendLine("Station registered");
                }

                if (!dto.IsHeartbeat)
                    _logFile.AppendLine($"There are {_dbContext.ClientStations.Count()} clients");
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
                _logFile.AppendLine("UnregisterClientAsync" + e.Message);
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
                _logFile.AppendLine("CleanClientStations" + e.Message);
                return -1;
            }
        }

        public async Task<int> CleanDeadClients(TimeSpan timeSpan)
        {
            try
            {
                DateTime noLaterThan = DateTime.Now - timeSpan;
                var deadStations = _dbContext.ClientStations.Where(s => s.LastConnectionTimestamp < noLaterThan).ToList();
                foreach (var deadStation in deadStations)
                {
                    _logFile.AppendLine($"Dead station {deadStation.StationId} registered by {deadStation.Username} will be removed.");
                    _dbContext.ClientStations.Remove(deadStation);
                }
                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("CleanDeadClients:" + e.Message);
                return -1;
            }
        }
    }
}