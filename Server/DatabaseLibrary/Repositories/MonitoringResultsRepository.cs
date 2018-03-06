using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class MonitoringResultsRepository
    {
        private readonly ISettings _settings;
        private readonly IMyLog _logFile;

        public MonitoringResultsRepository(ISettings settings, IMyLog logFile)
        {
            _settings = settings;
            _logFile = logFile;
        }

        public async Task<MeasurementWithSor> SaveMonitoringResultAsync(byte[] sorBytes, Measurement measurement)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var rtuStation = await dbContext.RtuStations.FirstOrDefaultAsync(r => r.RtuGuid == measurement.RtuId);
                    if (rtuStation == null)
                    {
                        _logFile.AppendLine($"Unknown RTU ({measurement.RtuId.First6()}) sent monitoring result. Ignored.");
                        return null;
                    }

                    var sorFile = new SorFile() { SorBytes = sorBytes };
                    dbContext.SorFiles.Add(sorFile);
                    await dbContext.SaveChangesAsync();

                    measurement.SorFileId = sorFile.Id;
                    dbContext.Measurements.Add(measurement);
                    await dbContext.SaveChangesAsync();
                    return new MeasurementWithSor(){Measurement = measurement, SorBytes = sorBytes};
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveMeasurementAsync: " + e.Message);
                return null;
            }
        }

    }
}