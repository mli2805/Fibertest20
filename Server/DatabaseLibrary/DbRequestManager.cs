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

        public async Task<TraceStatistics> GetTraceMeasurementsAsync(Guid traceId)
        {
            try
            {
                var dbContext = new MySqlContext();
                var result = new TraceStatistics
                {
                    Measurements = await dbContext.Measurements.Where(m => m.TraceId == traceId).ToListAsync(),
                    BaseRefs = new List<BaseRefForStats>()
                };
                var bb = await dbContext.BaseRefs.Where(b => b.TraceId == traceId).ToListAsync();
                foreach (var baseRef in bb)
                {
                    result.BaseRefs.Add(new BaseRefForStats()
                    {
                        BaseRefType = baseRef.BaseRefType,
                        AssignedAt = baseRef.SaveTimestamp,
                        AssignedBy = baseRef.UserId.ToString(),
                        BaseRefId = baseRef.BaseRefId,
                    });
                }
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetTraceMeasurementsAsync: " + e.Message);
                return null;
            }
        }

        public async Task<byte[]> GetSorBytesOfMeasurement(int sorFileId)
        {
            try
            {
                using (var dbContext = new MySqlContext())
                {
                    var result = await dbContext.SorFiles.Where(s => s.Id == sorFileId).FirstOrDefaultAsync();
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
