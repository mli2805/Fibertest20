using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class MeasurementsRepository
    {
        private readonly ISettings _settings;
        private readonly IMyLog _logFile;

        public MeasurementsRepository(ISettings settings, IMyLog logFile)
        {
            _settings = settings;
            _logFile = logFile;
        }

        public async Task<MeasurementsList> GetOpticalEventsAsync()
        {
            const int pageSize = 200;
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var actualEvents = await dbContext.Measurements.Where(m => m.EventStatus > EventStatus.JustMeasurementNotAnEvent).GroupBy(p => p.TraceId)
                        .Select(e => e.OrderByDescending(p => p.SorFileId).FirstOrDefault()).ToListAsync();
                    var page = await dbContext.Measurements.Where(m => m.EventStatus > EventStatus.JustMeasurementNotAnEvent).
                        OrderByDescending(p => p.SorFileId).Take(pageSize).ToListAsync();
                    return new MeasurementsList() {ActualMeasurements = actualEvents, PageOfLastMeasurements = page};
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetOpticalEvents: " + e.Message);
                return null;
            }
        }

        public async Task<TraceStatistics> GetTraceMeasurementsAsync(Guid traceId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
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
                            AssignedBy = baseRef.UserName,
                            BaseRefId = baseRef.BaseRefId,
                        });
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetTraceMeasurementsAsync: " + e.Message);
                return null;
            }
        }

        public async Task<byte[]> GetSorBytesOfBaseAsync(Guid baseRefId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var result = await dbContext.BaseRefs.Where(s => s.BaseRefId == baseRefId).FirstOrDefaultAsync();
                    return result?.SorBytes;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetSorBytesOfBaseAsync: " + e.Message);
                return null;
            }
        }

        public async Task<byte[]> GetSorBytesAsync(int sorFileId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var result = await dbContext.SorFiles.Where(s => s.Id == sorFileId).FirstOrDefaultAsync();
                    return result?.SorBytes;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetSorBytesAsync: " + e.Message);
                return null;
            }
        }

        public async Task<byte[]> GetSorBytesOfLastTraceMeasurementAsync(Guid traceId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var lastMeas = await dbContext.Measurements.OrderByDescending(m => m.Id).
                                            Where(m => m.TraceId == traceId).FirstOrDefaultAsync();
                    if (lastMeas == null)
                        return null;

                    var result = await dbContext.SorFiles.Where(s => s.Id == lastMeas.SorFileId).FirstOrDefaultAsync();
                    return result?.SorBytes;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetSorBytesOfLastTraceMeasurementAsync: " + e.Message);
                return null;
            }
        }

        public async Task<MeasurementWithSor> GetLastMeasurementForTraceAsync(Guid traceId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var measurement = await dbContext.Measurements.OrderByDescending(m => m.Id).
                                            Where(m => m.TraceId == traceId).FirstOrDefaultAsync();
                    var sor = await dbContext.SorFiles.FirstOrDefaultAsync(s => s.Id == measurement.SorFileId);
                    return new MeasurementWithSor(){Measurement = measurement, SorData = sor.SorBytes};
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetLastMeasurementForTraceAsync: " + e.Message);
                return null;
            }
        }

        public async Task<MeasurementUpdatedDto> SaveMeasurementChangesAsync(UpdateMeasurementDto dto)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    var measurement = await dbContext.Measurements.FirstOrDefaultAsync(m => m.SorFileId == dto.SorFileId);
                    if (measurement == null)
                    {
                        return new MeasurementUpdatedDto() { ReturnCode = ReturnCode.DbEntityToUpdateNotFound };
                    }

                    var clientStation = await dbContext.ClientStations.FirstOrDefaultAsync(s => s.ClientGuid == dto.ClientId);
                    if (clientStation == null)
                    {
                        return new MeasurementUpdatedDto() {ReturnCode = ReturnCode.DbError};
                    }

                    var userName = clientStation.UserName;

                    if (measurement.EventStatus != dto.EventStatus)
                    {
                        measurement.EventStatus = dto.EventStatus;
                        measurement.StatusChangedByUser = userName;
                        measurement.StatusChangedTimestamp = DateTime.Now;
                    }
                    measurement.Comment = dto.Comment;
                    await dbContext.SaveChangesAsync();

                    return new MeasurementUpdatedDto()
                    {
                        ReturnCode = ReturnCode.Ok, SorFileId = dto.SorFileId, StatusChangedTimestamp = measurement.StatusChangedTimestamp
                    };
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveMeasurementChangesAsync: " + e.Message);
                return null;
            }
        }

    }
}
