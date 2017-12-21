using System;
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

        public async Task<NetworkEventsList> GetNetworkEventsAsync(int afterIndex)
        {
            try
            {
                var dbContext = new FtDbContext();
                var result = new NetworkEventsList
                {
                    Events = await dbContext.NetworkEvents.Where(e => e.Id > afterIndex).ToListAsync()
                };
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetNetworkEvents: " + e.Message);
                return null;
            }
        }

    }
}