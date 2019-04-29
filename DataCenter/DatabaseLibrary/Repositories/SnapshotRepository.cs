using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class SnapshotRepository
    {
        private readonly ISettings _settings;
        private readonly IMyLog _logFile;

        public SnapshotRepository(ISettings settings, IMyLog logFile)
        {
            _settings = settings;
            _logFile = logFile;
        }

        // max_allowed_packet is 16M ???
        public async Task<int> AddSnapshotAsync(Guid aggregateId, int lastEventNumber, byte[] data)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    _logFile.AppendLine("Snapshot adding...");
                    var portion = 2 * 1024 * 1024;
                    for (int i = 0; i <= data.Length / portion; i++)
                    {
                        var payload = data.Skip(i * portion).Take(portion).ToArray();
                        var snapshot = new Snapshot()
                        {
                            AggregateId = aggregateId,
                            LastEventNumber = lastEventNumber,
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

        public async Task<Tuple<int, byte[]>> ReadSnapshotAsync(Guid aggregateId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    _logFile.AppendLine("Snapshot reading...");
                    await Task.Delay(1);
                    var portions = dbContext.Snapshots.Where(l => l.AggregateId == aggregateId);
                    var size = portions.Sum(p => p.Payload.Length);
                    var counter = 0;
                    byte[] data = new byte[size];
                    foreach (var t in portions)
                    {
                        t.Payload.CopyTo(data, counter);
                        counter = counter + t.Payload.Length;
                    }
                    var result = new Tuple<int, byte[]>(portions.First().LastEventNumber, data);
                    return result;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("ReadSnapshotAsync: " + e.Message);
                return null;
            }
        }

        public async Task<int> RemoveOldSnapshots(Guid newAggregateId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    _logFile.AppendLine("Snapshots removing...");
                    var oldSnapshots = dbContext.Snapshots.Where(f=>f.AggregateId != newAggregateId).ToList();
                    dbContext.Snapshots.RemoveRange(oldSnapshots);
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