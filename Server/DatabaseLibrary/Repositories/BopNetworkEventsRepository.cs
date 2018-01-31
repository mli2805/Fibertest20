using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class BopNetworkEventsRepository
    {
        private readonly ISettings _settings;
        private readonly IMyLog _logFile;

        public BopNetworkEventsRepository(ISettings settings, IMyLog logFile)
        {
            _settings = settings;
            _logFile = logFile;
        }

        public async Task<BopNetworkEventsList> GetBopNetworkEventsAsync()
        {
            const int pageSize = 200;
            try
            {
                using (var dbContext = new FtDbContext(_settings.MySqlConString))
                {
                    var actualEvents = await dbContext.BopNetworkEvents.GroupBy(p => p.BopId)
                        .Select(e => e.OrderByDescending(p => p.Id).FirstOrDefault()).ToListAsync();
                    var page = await dbContext.BopNetworkEvents.Take(pageSize).ToListAsync();
                    return new BopNetworkEventsList() { ActualEvents = actualEvents, PageOfLastEvents = page };
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetBopNetworkEventsAsync: " + e.Message);
                return null;
            }
        }
    }
}