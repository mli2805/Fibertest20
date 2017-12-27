using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class BopNetworkEventsRepository
    {
        private readonly IMyLog _logFile;

        public BopNetworkEventsRepository(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<BopNetworkEventsList> GetBopNetworkEventsAsync()
        {
            try
            {
                var actualEvents = await GetActualBopNetworkEventsAsync();
                var page = await GetPageOfLastBopNetworkEventsAsync();
                return new BopNetworkEventsList() { ActualEvents = actualEvents, PageOfLastEvents = page };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetBopNetworkEventsAsync: " + e.Message);
                return null;
            }
        }

        private async Task<List<BopNetworkEvent>> GetActualBopNetworkEventsAsync()
        {
            try
            {
                var dbContext = new FtDbContext();
                var events = await dbContext.BopNetworkEvents.GroupBy(p => p.BopId)
                    .Select(e => e.OrderByDescending(p => p.Id).FirstOrDefault()).ToListAsync();
                return events;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetActualBopNetworkEventsAsync: " + e.Message);
                return null;
            }
        }

        private async Task<List<BopNetworkEvent>> GetPageOfLastBopNetworkEventsAsync()
        {
            const int pageSize = 200;
            try
            {
                var dbContext = new FtDbContext();
                var pageOfLastEvents = await dbContext.BopNetworkEvents.Take(pageSize).ToListAsync();
                return pageOfLastEvents;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetPageOfLastBopNetworkEventsAsync: " + e.Message);
                return null;
            }
        }
    }
}