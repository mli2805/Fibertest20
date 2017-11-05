using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
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

        private async Task<int> CleanRtuStations()
        {
            try
            {
                var dbContext = new MySqlContext();
                dbContext.RtuStations.RemoveRange(dbContext.RtuStations);
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("CleanRtuStations: " + e.Message);
                return -1;
            }
        }

        private async Task<int> InitializeRtuStationTable(List<Rtu> initializedRtus)
        {
            try
            {
                var dbContext = new MySqlContext();
                foreach (var rtu in initializedRtus)
                {
                    dbContext.RtuStations.Add(MapperToRtuStation.Map(rtu));
                    _logFile.AppendLine($"RTU {rtu.Id.First6()}  main address {rtu.MainChannel.ToStringA()}");
                }
                var savedRows = await dbContext.SaveChangesAsync();
                _logFile.AppendLine($"{savedRows} RTU found.");
                return savedRows;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("InitializeRtuStationTable: " + e.Message);
                return -1;
            }
        }

        public async Task<int> InitManager(List<Rtu> initializedRtu)
        {
            await CleanRtuStations();
            return await InitializeRtuStationTable(initializedRtu);
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

        public async Task<int> RemoveRtuAsync(Guid rtuId)
        {
            try
            {
                var dbContext = new MySqlContext();
                var rtu = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == rtuId);
                if (rtu != null)
                {
                    dbContext.RtuStations.Remove(rtu);
                    _logFile.AppendLine("RTU removed.");
                    return await dbContext.SaveChangesAsync();
                }
                else
                {
                    _logFile.AppendLine($"RTU with id {rtuId.First6()} not found");
                    return 0;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterRtuAsync: " + e.Message);
                return -1;
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

        public async Task<int> NotifyAboutRtuThatChangedAvailability(TimeSpan timeSpan)
        {
            try
            {
                var dbContext = new MySqlContext();
                DateTime noLaterThan = DateTime.Now - timeSpan;
                var changes = new RtuWithChannelChangesList();

                var stationsWithExpiredMainChannel = dbContext.RtuStations.
                    Where(s => s.LastConnectionByMainAddressTimestamp < noLaterThan && s.IsMainAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithExpiredMainChannel)
                {
                    rtuStation.IsMainAddressOkDuePreviousCheck = false;
                    changes.AddOrUpdate(rtuStation, true, ChannelStateChanges.Broken);
                }
                var stationsWithRecoveredMainChannel = dbContext.RtuStations.
                    Where(s => s.LastConnectionByMainAddressTimestamp >= noLaterThan && !s.IsMainAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithRecoveredMainChannel)
                {
                    rtuStation.IsMainAddressOkDuePreviousCheck = true;
                    changes.AddOrUpdate(rtuStation, true, ChannelStateChanges.Recovered);
                }

                var stationsWithExpiredReserveChannel = dbContext.RtuStations
                    .Where(s => s.IsReserveAddressSet &&
                        s.LastConnectionByReserveAddressTimestamp < noLaterThan && s.IsReserveAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithExpiredReserveChannel)
                {
                    rtuStation.IsReserveAddressOkDuePreviousCheck = false;
                    changes.AddOrUpdate(rtuStation, false, ChannelStateChanges.Broken);
                }
                var stationsWithRecoveredReserveChannel = dbContext.RtuStations
                    .Where(s => s.IsReserveAddressSet &&
                        s.LastConnectionByReserveAddressTimestamp >= noLaterThan && !s.IsReserveAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithRecoveredReserveChannel)
                {
                    rtuStation.IsReserveAddressOkDuePreviousCheck = true;
                    changes.AddOrUpdate(rtuStation, false, ChannelStateChanges.Recovered);
                }

                if (changes.List.Count != 0)
                {
                    foreach (var station in changes.List)
                    {
                        _logFile.AppendLine(station.Report());
                        // TODO notify client
                    }
                    return await dbContext.SaveChangesAsync();
                }

                return 0;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("NotifyAboutRtuThatChangedAvailability:" + e.Message);
                return -1;
            }
        }

    }
}