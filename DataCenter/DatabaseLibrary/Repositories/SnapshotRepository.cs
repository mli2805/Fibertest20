using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class SnapshotRepository
    {
        private readonly IParameterizer _parameterizer;
        private readonly IMyLog _logFile;

        public SnapshotRepository(IParameterizer parameterizer, IMyLog logFile)
        {
            _parameterizer = parameterizer;
            _logFile = logFile;
        }

        // max_allowed_packet is 16M ???
        public async Task<int> AddSnapshotAsync(Guid graphDbVersionId, int lastEventNumber, DateTime lastEventDate, byte[] data)
        {
            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    _logFile.AppendLine("Snapshot adding...");
                    var portion = 2 * 1024 * 1024;
                    for (int i = 0; i <= data.Length / portion; i++)
                    {
                        var payload = data.Skip(i * portion).Take(portion).ToArray();
                        var snapshot = new Snapshot()
                        {
                            StreamIdOriginal = graphDbVersionId,
                            LastEventNumber = lastEventNumber,
                            LastEventDate = lastEventDate,
                            Payload = payload
                        };
                        dbContext.Snapshots.Add(snapshot);
                        var result = await dbContext.SaveChangesAsync();
                        if (result == 1)
                            _logFile.AppendLine($"{i+1} portion,   {payload.Length} size");
                        else return -1;
                    }
                    return data.Length / portion;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AddSnapshotAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<Tuple<int, byte[], DateTime>> ReadSnapshotAsync(Guid graphDbVersionId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    _logFile.AppendLine("Snapshot reading...");
                    var portions = await dbContext.Snapshots.Where(l => l.StreamIdOriginal == graphDbVersionId).ToListAsync();
                    if (!portions.Any())
                    {
                        _logFile.AppendLine("No snapshots");
                        return new Tuple<int, byte[], DateTime>(0, null, DateTime.MinValue);
                    }
                    var size = portions.Sum(p => p.Payload.Length);
                    var offset = 0;
                    byte[] data = new byte[size];
                    foreach (var t in portions)
                    {
                        t.Payload.CopyTo(data, offset);
                        offset = offset + t.Payload.Length;
                    }
                    var result = new Tuple<int, byte[], DateTime>(portions.First().LastEventNumber, data, portions.First().LastEventDate);
                    _logFile.AppendLine($@"Snapshot size {result.Item2.Length:0,0} bytes.    Number of last event in snapshot {result.Item1:0,0}.");
                    return result;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("ReadSnapshotAsync: " + e.Message);
                return new Tuple<int, byte[], DateTime>(0, null, DateTime.MinValue);
            }
        }

        public async Task<SnapshotParamsDto> GetSnapshotParams(int lastIncludedEvent)
        {
            try
            {
                await Task.Delay(1);
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    var result = new SnapshotParamsDto
                    {
                        PortionsCount = dbContext.Snapshots.Count(l => l.LastEventNumber == lastIncludedEvent),
                        Size = dbContext.Snapshots.Where(l => l.LastEventNumber == lastIncludedEvent)
                            .Select(r => r.Payload.Length).AsEnumerable().Sum()
                    };
                    return result;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetSnapshotParams: " + e.Message);
                return null;
            }
        }

        public async Task<byte[]> GetSnapshotPortion(int portionOrdinal)
        {
            try
            {
                await Task.Delay(1);
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    var firstId = dbContext.Snapshots.First().Id;
                    return dbContext.Snapshots.First(p => p.Id == firstId + portionOrdinal).Payload;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetSnapshotPortion: " + e.Message);
                return null;
            }
        }


        public async Task<int> RemoveOldSnapshots()
        {
            try
            {
                using (var dbContext = new FtDbContext(_parameterizer.Options))
                {
                    _logFile.AppendLine("Snapshots removing...");
                    var maxLastEventNumber = dbContext.Snapshots.Max(f => f.LastEventNumber); 
                    var oldSnapshotPortions = dbContext.Snapshots.Where(f=>f.LastEventNumber != maxLastEventNumber).ToList();
                    dbContext.Snapshots.RemoveRange(oldSnapshotPortions);
                    return await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RemoveOldSnapshots: " + e.Message);
                return -1;
            }
        }


    }
}