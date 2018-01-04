using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class MonitoringResultsRepository
    {
        private readonly IMyLog _logFile;
        private readonly MeasurementFactory _measurementFactory;

        public MonitoringResultsRepository(IMyLog logFile, MeasurementFactory measurementFactory)
        {
            _logFile = logFile;
            _measurementFactory = measurementFactory;
        }

        public async Task<Measurement> SaveMonitoringResultAsync(MonitoringResultDto result)
        {
            try
            {
                var dbContext = new FtDbContext();

                var sorFile = new SorFile() { SorBytes = result.SorData };
                dbContext.SorFiles.Add(sorFile);
                await dbContext.SaveChangesAsync();

                var measurement = _measurementFactory.Create(result, sorFile.Id);
                dbContext.Measurements.Add(measurement);
                await dbContext.SaveChangesAsync();
                return measurement;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveMeasurementAsync: " + e.Message);
                return null;
            }
        }

    }
}