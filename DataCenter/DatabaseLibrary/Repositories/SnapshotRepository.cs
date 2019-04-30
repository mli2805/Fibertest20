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
        public async Task<int> AddSnapshotAsync(Guid graphDbVersionId, int lastEventNumber, byte[] data)
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
                            AggregateId = graphDbVersionId,
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

        public async Task<Tuple<int, byte[]>> ReadSnapshotAsync(Guid graphDbVersionId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
                {
                    _logFile.AppendLine("Snapshot reading...");
                    await Task.Delay(1);
                    var portions = dbContext.Snapshots.Where(l => l.AggregateId == graphDbVersionId);
                    if (!portions.Any())
                    {
                        _logFile.AppendLine("No snapshots");
                        return new Tuple<int, byte[]>(0, null);
                    }
                    var size = portions.Sum(p => p.Payload.Length);
                    var counter = 0;
                    byte[] data = new byte[size];
                    foreach (var t in portions)
                    {
                        t.Payload.CopyTo(data, counter);
                        counter = counter + t.Payload.Length;
                    }
                    var result = new Tuple<int, byte[]>(portions.First().LastEventNumber, data);
                    _logFile.AppendLine($@"Snapshot size {result.Item2.Length:0,0} bytes.    Number of last event in snapshot {result.Item1:0,0}.");
                    return result;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("ReadSnapshotAsync: " + e.Message);
                return new Tuple<int, byte[]>(0, null);
            }
        }

        public async Task<int> RemoveOldSnapshots()
        {
            try
            {
                using (var dbContext = new FtDbContext(_settings.Options))
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