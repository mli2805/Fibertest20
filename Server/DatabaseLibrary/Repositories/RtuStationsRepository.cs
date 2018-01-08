using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class RtuStationsRepository
    {
        private readonly IMyLog _logFile;

        public RtuStationsRepository(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<int> RegisterRtuAsync(RtuStation rtuStation)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    var rtu = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == rtuStation.RtuGuid);
                    if (rtu == null)
                    {
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
                using (var dbContext = new FtDbContext())
                {
                    var rtu = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == rtuId);
                    if (rtu != null)
                    {
                        dbContext.RtuStations.Remove(rtu);
                        await dbContext.SaveChangesAsync();
                        _logFile.AppendLine("RTU removed.");
                        return null;
                    }

                    var message = $"RTU with id {rtuId.First6()} not found";
                    _logFile.AppendLine(message);
                    return message;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RemoveRtuAsync: " + e.Message);
                return e.Message;
            }
        }

        public async Task<DoubleAddress> GetRtuAddresses(Guid rtuId)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    var rtu = await dbContext.RtuStations.FirstOrDefaultAsync(r => r.RtuGuid == rtuId);
                    if (rtu != null)
                    {
                        return rtu.GetRtuDoubleAddress();
                    }

                    _logFile.AppendLine($"RTU with id {rtuId.First6()} not found");
                    return null;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetRtuAddresses: " + e.Message);
                return null;
            }
        }

        public async Task<int> RegisterRtuHeartbeatAsync(RtuChecksChannelDto dto)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
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
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterRtuHeartbeatAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<List<RtuStation>> GetAllRtuStations()
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    return await dbContext.RtuStations.ToListAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetAllRtuStations: " + e.Message);
                return null;
            }
        }

        public async Task<int> SaveAvailabilityChanges(List<RtuStation> changedStations)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    foreach (var changedStation in changedStations)
                    {
                        var rtuStation = dbContext.RtuStations.First(r => r.RtuGuid == changedStation.RtuGuid);
                        dbContext.RtuStations.Remove(rtuStation);
                        dbContext.RtuStations.Add(changedStation);
                    }
                    return await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveAvailabilityChanges: " + e.Message);
                return -1;
            }
        }
    }
}