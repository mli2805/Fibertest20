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
            const int pageSize = 200;
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    var actualEvents = await dbContext.NetworkEvents.GroupBy(p => p.RtuId)
                        .Select(e => e.OrderByDescending(p => p.Id).FirstOrDefault()).ToListAsync();
                    var page = await dbContext.NetworkEvents.Take(pageSize).ToListAsync();
                    return new NetworkEventsList() {ActualEvents = actualEvents, PageOfLastEvents = page};
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetNetworkEventsAsync: " + e.Message);
                return null;
            }
        }

        public async Task<int> SaveNetworkEventAsync(List<NetworkEvent> networkEvents)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    dbContext.NetworkEvents.AddRange(networkEvents);
                    return await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveNetworkEventAsync: " + e.Message);
                return 0;
            }
        }

    }
}