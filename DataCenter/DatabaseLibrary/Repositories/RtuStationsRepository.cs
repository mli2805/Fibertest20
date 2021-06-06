using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class RtuStationsRepository
    {
        private readonly IParameterizer _parameterizer;
        private readonly IMyLog _logFile;

        public RtuStationsRepository(IParameterizer parameterizer, IMyLog logFile)
        {
            _parameterizer = parameterizer;
            _logFile = logFile;
        }

        public async Task<int> RegisterRtuAsync(RtuStation rtuStation)
        {
            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    var previousRtuStationRow = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == rtuStation.RtuGuid);
                    if (previousRtuStationRow == null)
                        dbContext.RtuStations.Add(rtuStation);
                    else
                    {
                        previousRtuStationRow.Version = rtuStation.Version;
                        previousRtuStationRow.MainAddress = rtuStation.MainAddress;
                        previousRtuStationRow.MainAddressPort = rtuStation.MainAddressPort;
                        previousRtuStationRow.IsReserveAddressSet = rtuStation.IsReserveAddressSet;
                        previousRtuStationRow.ReserveAddress = rtuStation.ReserveAddress;
                        previousRtuStationRow.ReserveAddressPort = rtuStation.ReserveAddressPort;
                    }

                    _logFile.AppendLine($"RtuStation {rtuStation.RtuGuid.First6()} successfully registered with main address {rtuStation.MainAddress}.");
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
                using (var dbContext = new FtDbContext(_parameterizer.Options))
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
                using (var dbContext = new FtDbContext(_parameterizer.Options))
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
                using (var dbContext = new FtDbContext(_parameterizer.Options))
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

        public async Task<bool> IsRtuExist(Guid rtuId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    var rtu = await dbContext.RtuStations.FirstOrDefaultAsync(r => r.RtuGuid == rtuId);
                    if (rtu != null) return true;
                    _logFile.AppendLine($"Unknown RTU {rtuId.First6()}");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("IsRtuExist: " + e.Message);
                return false;
            }
        }

        public async Task<List<RtuStation>> GetAllRtuStations()
        {
            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
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
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    foreach (var changedStation in changedStations)
                    {
                        var rtuStation = dbContext.RtuStations.First(r => r.RtuGuid == changedStation.RtuGuid);
                        Cop(changedStation, rtuStation);
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

        private void Cop(RtuStation source, RtuStation destination)
        {
            destination.IsMainAddressOkDuePreviousCheck = source.IsMainAddressOkDuePreviousCheck;
            destination.IsReserveAddressOkDuePreviousCheck = source.IsReserveAddressOkDuePreviousCheck;
            destination.IsReserveAddressSet = source.IsReserveAddressSet;
            destination.LastConnectionByMainAddressTimestamp = source.LastConnectionByMainAddressTimestamp;
            destination.LastConnectionByReserveAddressTimestamp = source.LastConnectionByReserveAddressTimestamp;
            destination.MainAddress = source.MainAddress;
            destination.MainAddressPort = source.MainAddressPort;
            destination.ReserveAddress = source.ReserveAddress;
            destination.ReserveAddressPort = source.ReserveAddressPort;
            destination.RtuGuid = source.RtuGuid;
            destination.Version = source.Version;
        }
    }
}