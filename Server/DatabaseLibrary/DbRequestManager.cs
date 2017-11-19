using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class DbRequestManager
    {
        private readonly IMyLog _logFile;

        public DbRequestManager(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<List<OpticalEvent>> GetOpticalEventsAsync(int afterIndex)
        {
            try
            {
                var dbContext = new MySqlContext();
                var events = await dbContext.OpticalEvents.Where(e => e.Id > afterIndex).ToListAsync();
                return events;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetOpticalEvents: " + e.Message);
                return null;
            }
        }

        public async Task<NetworkEventsList> GetNetworkEventsAsync(int afterIndex)
        {
            try
            {
                var dbContext = new MySqlContext();
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

        public async Task<byte[]> GetSorBytesOfMeasurement(Guid measurementId)
        {
            try
            {
                using (var dbContext = new MySqlContext())
                {
                    var result = await dbContext.SorFiles.Where(s => s.MeasurementId == measurementId).FirstOrDefaultAsync();
                    return result?.SorBytes;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetSorBytesOfMeasurement: " + e.Message);
                return null;
            }
        }
    }
}
