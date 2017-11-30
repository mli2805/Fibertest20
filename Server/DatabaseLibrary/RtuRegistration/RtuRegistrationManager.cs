using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class RtuRegistrationManager
    {
        private readonly IMyLog _logFile;
        //        private readonly IFibertestDbContext _dbContext;

        public RtuRegistrationManager(IMyLog logFile)
        {
            _logFile = logFile;
        }



        public async Task<int> RegisterRtuAsync(RtuInitializedDto dto)
        {
            try
            {
                var dbContext = new MySqlContext();
                var rtu = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == dto.RtuId);
                if (rtu == null)
                {
                    var rtuStation = MapperToRtuStation.Map(dto);
                    dbContext.RtuStations.Add(rtuStation);
                    _logFile.AppendLine("New RTU registered.");
                }
                else
                {
                    rtu.LastConnectionByMainAddressTimestamp = DateTime.Now;
                    _logFile.AppendLine("Existing RTU re-registered");
                }
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterRtuAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<string> RemoveRtuAsync(Guid rtuId)
        {
            try
            {
                var dbContext = new MySqlContext();
                var rtu = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == rtuId);
                if (rtu != null)
                {
                    dbContext.RtuStations.Remove(rtu);
                    _logFile.AppendLine("RTU removed.");
                    await dbContext.SaveChangesAsync();
                    return null;
                }
                else
                {
                    var message = $"RTU with id {rtuId.First6()} not found";
                    _logFile.AppendLine(message);
                    return message;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterRtuAsync: " + e.Message);
                return e.Message;
            }
        }

        public Task<DoubleAddress> GetRtuAddresses(Guid rtuId)
        {
            try
            {
                var dbContext = new MySqlContext();
                var rtu = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == rtuId);
                if (rtu != null)
                {
                    return Task.FromResult(rtu.GetRtuDoubleAddress());
                }
                else
                {
                    _logFile.AppendLine($"RTU with id {rtuId.First6()} not found");
                    return null;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterRtuAsync: " + e.Message);
                return null;
            }
        }


        public async Task<int> RegisterRtuHeartbeatAsync(RtuChecksChannelDto dto)
        {
            try
            {
                var dbContext = new MySqlContext();
                var rtu = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == dto.RtuId);
                if (rtu == null)
                {
                    _logFile.AppendLine($"Unknown RTU's {dto.RtuId.First6()} heartbeat.");
                }
                else
                {
                    if (dto.IsMainChannel)
                        rtu.LastConnectionByMainAddressTimestamp = DateTime.Now;
                    else
                        rtu.LastConnectionByReserveAddressTimestamp = DateTime.Now;
                    rtu.Version = dto.Version;
                }
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterRtuHeartbeatAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<RtuWithChannelChangesList> GetAndSaveRtuStationsAvailabilityChanges(TimeSpan rtuHeartbeatPermittedGap)
        {
            var changes = new RtuWithChannelChangesList();
            try
            {
                var dbContext = new MySqlContext();
                DateTime noLaterThan = DateTime.Now - rtuHeartbeatPermittedGap;

                var stationsWithExpiredMainChannel = dbContext.RtuStations.
                    Where(s => s.LastConnectionByMainAddressTimestamp < noLaterThan && s.IsMainAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithExpiredMainChannel)
                {
                    rtuStation.IsMainAddressOkDuePreviousCheck = false;
                    changes.AddOrUpdate(rtuStation, Channel.Main, ChannelStateChanges.Broken);
                }
                var stationsWithRecoveredMainChannel = dbContext.RtuStations.
                    Where(s => s.LastConnectionByMainAddressTimestamp >= noLaterThan && !s.IsMainAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithRecoveredMainChannel)
                {
                    rtuStation.IsMainAddressOkDuePreviousCheck = true;
                    changes.AddOrUpdate(rtuStation, Channel.Main, ChannelStateChanges.Recovered);
                }

                var stationsWithExpiredReserveChannel = dbContext.RtuStations
                    .Where(s => s.IsReserveAddressSet &&
                        s.LastConnectionByReserveAddressTimestamp < noLaterThan && s.IsReserveAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithExpiredReserveChannel)
                {
                    rtuStation.IsReserveAddressOkDuePreviousCheck = false;
                    changes.AddOrUpdate(rtuStation, Channel.Reserve, ChannelStateChanges.Broken);
                }
                var stationsWithRecoveredReserveChannel = dbContext.RtuStations
                    .Where(s => s.IsReserveAddressSet &&
                        s.LastConnectionByReserveAddressTimestamp >= noLaterThan && !s.IsReserveAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithRecoveredReserveChannel)
                {
                    rtuStation.IsReserveAddressOkDuePreviousCheck = true;
                    changes.AddOrUpdate(rtuStation, Channel.Reserve, ChannelStateChanges.Recovered);
                }
                await dbContext.SaveChangesAsync();
                return changes;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetAndSaveRtuStationsAvailabilityChanges:" + e.Message);
            }
            return new RtuWithChannelChangesList();
        }

    }
}