using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class NetworkEventsRepository
    {
        private readonly IMyLog _logFile;

        public NetworkEventsRepository(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<NetworkEventsList> GetNetworkEventsAsync()
        {
            try
            {
                var actualEvents = await GetActualNetworkEventsAsync();
                var page = await GetPageOfLastNetworkEventsAsync();
                return new NetworkEventsList() {ActualEvents = actualEvents, PageOfLastEvents = page};
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetNetworkEvents: " + e.Message);
                return null;
            }
        }

        private async Task<List<NetworkEvent>> GetActualNetworkEventsAsync()
        {
            try
            {
                var dbContext = new FtDbContext();
                var events = await dbContext.NetworkEvents.GroupBy(p => p.RtuId)
                    .Select(e => e.OrderByDescending(p => p.Id).FirstOrDefault()).ToListAsync();
                return events;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetNetworkEvents: " + e.Message);
                return null;
            }
        }

        private async Task<List<NetworkEvent>> GetPageOfLastNetworkEventsAsync()
        {
            const int pageSize = 200;
            try
            {
                var dbContext = new FtDbContext();
                var pageOfLastEvents = await dbContext.NetworkEvents.Take(pageSize).ToListAsync();
                return pageOfLastEvents;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetNetworkEvents: " + e.Message);
                return null;
            }
        }

        public async Task<int> SaveNetworkEventAsync(List<NetworkEvent> networkEvents)
        {
            try
            {
                var dbContext = new FtDbContext();
                dbContext.NetworkEvents.AddRange(networkEvents);
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetNetworkEvents: " + e.Message);
                return 0;
            }
        }

    }
}