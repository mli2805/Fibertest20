using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class MonitoringResultsRepository
    {
        private readonly IMyLog _logFile;

        public MonitoringResultsRepository(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<MeasurementWithSor> SaveMonitoringResultAsync(byte[] sorBytes, Measurement measurement)
        {
            try
            {
                using (var dbContext = new FtDbContext())
                {
                    var sorFile = new SorFile() { SorBytes = sorBytes };
                    dbContext.SorFiles.Add(sorFile);
                    await dbContext.SaveChangesAsync();

                    measurement.SorFileId = sorFile.Id;
                    dbContext.Measurements.Add(measurement);
                    await dbContext.SaveChangesAsync();
                    return new MeasurementWithSor(){Measurement = measurement, SorData = sorBytes};
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