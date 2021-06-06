using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using NEventStore;
using Newtonsoft.Json;

namespace Iit.Fibertest.Client
{
    public class LocalDbManager : ILocalDbManager
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;

        private string _serverAddress;
        private string _filename;
        private string _connectionString;

        public LocalDbManager(IniFile iniFile, IMyLog logFile,
            CurrentDatacenterParameters currentDatacenterParameters)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _currentDatacenterParameters = currentDatacenterParameters;
        }

        public void Initialize()
        {
            var serverDoubleAddress = _iniFile.ReadDoubleAddress(11840);
            _serverAddress = serverDoubleAddress.Main.GetAddress();

            _filename = GetFullDbFilename(_currentDatacenterParameters.StreamIdOriginal);
            _logFile.AppendLine($@"Db full filename: {_filename}");

            _connectionString = $@"Data Source={_filename}; Version=3;";
            _logFile.AppendLine($@"Connection string: <<{_connectionString}>>");

            CreateIfNeeded();
        }

        private string GetFullDbFilename(Guid streamIdOriginal)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            _logFile.AppendLine($@"Application path: {appPath}");

            return FileOperations.GetParentFolder(appPath) +
                   $@"\Cache\GraphDb\{_serverAddress}\{streamIdOriginal.ToString()}.sqlite3";
        }

        public async Task SaveEvents(string[] jsons, int eventId)
        {
            try
            {
                using (var dbContext = new LocalDbSqliteContext(_connectionString))
                {
                    foreach (var json in jsons)
                    {
                        dbContext.EsEvents.Add(new EsEvent() { EventId = eventId, Json = json });
                        eventId++;
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"SaveEvents : {e.Message}");
            }
        }

        public async Task<CacheParameters> GetCacheParameters()
        {
            try
            {
                await Task.Delay(1);
                using (var dataContext = new LocalDbSqliteContext(_connectionString))
                {
                    var result = new CacheParameters();
                    var snapshot = dataContext.EsSnapshots.FirstOrDefault();
                    result.SnapshotLastEventNumber = snapshot?.LastIncludedEvent ?? 0;

                    var count = dataContext.EsEvents.Count();
                    result.LastEventNumber = count + result.SnapshotLastEventNumber;

                    if (count > 0)
                    {
                        var esEvent = dataContext.EsEvents.FirstOrDefault(m => m.Id == count); // last event
                        if (esEvent != null)
                        {
                            var msg = (EventMessage)JsonConvert.DeserializeObject(esEvent.Json,
                                JsonSerializerSettings);
                            if (msg != null)
                                result.LastEventTimestamp = (DateTime)msg.Headers[@"Timestamp"];
                        }
                    }

                    _logFile.AppendLine($@"Cache: last in snapshot {result.SnapshotLastEventNumber
                                   }, last event {result.LastEventNumber} at {result.LastEventTimestamp:O}");
                    return result;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"GetCacheParameters : {e.Message}");
                return null;
            }
        }

        public async Task<string[]> LoadEvents(int lastEventInSnapshot)
        {
            try
            {

                // SQLite do not work asynchronously (event though there is a ToArrayAsync function)
                // https://stackoverflow.com/questions/42982444/entity-framework-core-sqlite-async-requests-are-actually-synchronous

                return await Task.Factory.StartNew(() =>
                {
                    using (var dataContext = new LocalDbSqliteContext(_connectionString))
                    {
                        return dataContext.EsEvents
                            .Where(e => e.EventId > lastEventInSnapshot)
                            .Select(j => j.Json)
                            .ToArray();
                        // return dataContext.EsEvents.Select(j => j.Json).Skip(_currentDatacenterParameters.SnapshotLastEvent).ToArray();
                    }
                });
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"LoadEvents : {e.Message}");
                return new string[0];
            }
        }

        public async Task<byte[]> LoadSnapshot(int lastEventInSnapshotOnServer)
        {
            try
            {
                await Task.Delay(1);
                using (var dataContext = new LocalDbSqliteContext(_connectionString))
                {
                    var portions = dataContext.EsSnapshots.Where(p => p.LastIncludedEvent == lastEventInSnapshotOnServer).ToArray();
                    if (portions.Length == 0)
                    {
                        _logFile.AppendLine($@"Snapshot with last event number {
                                     lastEventInSnapshotOnServer } not found in cache.");
                        return new byte[0];
                    }
                    _logFile.AppendLine($@"From cache. {portions.Length} records");
                    var result = new byte[portions.Sum(p => p.Snapshot.Length)];
                    var offset = 0;
                    foreach (var portion in portions)
                    {
                        portion.Snapshot.CopyTo(result, offset);
                        offset = offset + portion.Snapshot.Length;
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"LoadSnapshot : {e.Message}");
                return null;
            }
        }

        public async Task<int> SaveSnapshot(byte[] portion)
        {
            try
            {
                using (var dataContext = new LocalDbSqliteContext(_connectionString))
                {
                    dataContext.EsSnapshots.Add(new EsSnapshot()
                    {
                        LastIncludedEvent = _currentDatacenterParameters.SnapshotLastEvent,
                        Snapshot = portion,
                    });
                    return await dataContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"SaveSnapshot : {e.Message}");
                return -1;
            }
        }

        public async Task<bool> RecreateCacheDb()
        {
            try
            {
                SqliteOperations.DropCacheTables(_filename, _logFile);
                await Task.Delay(20);
                SqliteOperations.CreateCacheTables(_filename, _logFile);
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"RecreateCache : {e.Message}");
                return false;
            }
        }

        private void CreateIfNeeded()
        {
            var s = AppDomain.CurrentDomain.BaseDirectory + $@"..\Cache\GraphDb\{_serverAddress}";
            if (!Directory.Exists(s))
                Directory.CreateDirectory(s);

            if (File.Exists(_filename)) return;

            SQLiteConnection.CreateFile(_filename);
            SqliteOperations.CreateCacheTables(_filename, _logFile);
        }
    }
}