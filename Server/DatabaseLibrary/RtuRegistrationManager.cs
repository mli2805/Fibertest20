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
    public enum ChannelStateChanges
    {
        Broken,
        TheSame,
        Recovered,
    }

    public class RtuWithChannelChanges
    {
        public Guid RtuId { get; set; }
        public ChannelStateChanges MainChannel { get; set; } = ChannelStateChanges.TheSame;
        public ChannelStateChanges ReserveChannel { get; set; } = ChannelStateChanges.TheSame;

        public string Report()
        {
            var mainChannel = MainChannel == ChannelStateChanges.TheSame
                ? ""
                : MainChannel == ChannelStateChanges.Broken
                    ? "Main channel is Broken"
                    : "Main channel Recovered";

            var reserveChannel = ReserveChannel == ChannelStateChanges.TheSame
                ? ""
                : ReserveChannel == ChannelStateChanges.Broken
                    ? "Reserve channel is Broken"
                    : "Reserve channel Recovered";

            return $"RTU {RtuId.First6()} " + mainChannel + reserveChannel;
        }
    }

    public class RtuWithChannelChangesList
    {
        public List<RtuWithChannelChanges> List = new List<RtuWithChannelChanges>();

        public void AddOrUpdate(RtuStation rtuStation, bool isMainChannel, ChannelStateChanges changes)
        {
            var rtu = List.FirstOrDefault(r => r.RtuId == rtuStation.StationId);
            if (rtu == null)
            {
                var rtuWithChannelChanges = new RtuWithChannelChanges() { RtuId = rtuStation.StationId };
                if (isMainChannel)
                    rtuWithChannelChanges.MainChannel = changes;
                else
                    rtuWithChannelChanges.ReserveChannel = changes;
                List.Add(rtuWithChannelChanges);
            }
            else
            {
                if (isMainChannel)
                    rtu.MainChannel = changes;
                else
                    rtu.ReserveChannel = changes;
            }
        }
    }

    public class RtuRegistrationManager
    {
        private readonly IMyLog _logFile;
        private readonly IFibertestDbContext _dbContext;

        public RtuRegistrationManager(IMyLog logFile, IFibertestDbContext dbContext)
        {
            _logFile = logFile;
            _dbContext = dbContext;
        }

        private async Task<int> CleanRtuStations()
        {
            try
            {
                _dbContext.RtuStations.RemoveRange(_dbContext.RtuStations);
                return await _dbContext.SaveChangesAsync();
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
                foreach (var rtu in initializedRtus)
                {
                    _dbContext.RtuStations.Add(MapperToRtuStation.Map(rtu));
                    _logFile.AppendLine($"RTU {rtu.Id.First6()}  main address {rtu.MainChannel.ToStringA()}");
                }
                _logFile.AppendLine($"{initializedRtus.Count} RTU found.");
                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("InitializeRtuStationTable: " + e.Message);
                return -1;
            }
        }

        public async Task<int> Init(List<Rtu> initializedRtu)
        {
            await CleanRtuStations();
            return await InitializeRtuStationTable(initializedRtu);
        }

        public async Task<int> RegisterRtuAsync(RtuInitializedDto dto)
        {
            try
            {
                var rtu = _dbContext.RtuStations.FirstOrDefault(r => r.StationId == dto.RtuId);
                if (rtu == null)
                {
                    _dbContext.RtuStations.Add(MapperToRtuStation.Map(dto));
                    _logFile.AppendLine("New RTU registered.");
                }
                else
                {
                    rtu.LastConnectionByMainAddressTimestamp = DateTime.Now;
                    _logFile.AppendLine("Existed RTU re-registered");
                }
                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterRtuAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<int> NotifyAboutRtuThatChangedAvailability(TimeSpan timeSpan)
        {
            try
            {
                DateTime noLaterThan = DateTime.Now - timeSpan;
                var changes = new RtuWithChannelChangesList();

                var stationsWithExpiredMainChannel = _dbContext.RtuStations.
                    Where(s => s.LastConnectionByMainAddressTimestamp < noLaterThan && s.IsMainAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithExpiredMainChannel)
                {
                    rtuStation.IsMainAddressOkDuePreviousCheck = false;
                    changes.AddOrUpdate(rtuStation, true, ChannelStateChanges.Broken);
                }
                var stationsWithRecoveredMainChannel = _dbContext.RtuStations.
                    Where(s => s.LastConnectionByMainAddressTimestamp >= noLaterThan && !s.IsMainAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithRecoveredMainChannel)
                {
                    rtuStation.IsMainAddressOkDuePreviousCheck = true;
                    changes.AddOrUpdate(rtuStation, true, ChannelStateChanges.Recovered);
                }

                var stationsWithExpiredReserveChannel = _dbContext.RtuStations
                    .Where(s => s.IsReserveAddressSet &&
                        s.LastConnectionByReserveAddressTimestamp < noLaterThan && s.IsReserveAddressOkDuePreviousCheck).ToList();
                foreach (var rtuStation in stationsWithExpiredReserveChannel)
                {
                    rtuStation.IsReserveAddressOkDuePreviousCheck = false;
                    changes.AddOrUpdate(rtuStation, false, ChannelStateChanges.Broken);
                }
                var stationsWithRecoveredReserveChannel = _dbContext.RtuStations
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
                    }
                    return await _dbContext.SaveChangesAsync();
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