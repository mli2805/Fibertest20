using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class LocalDbManager : ILocalDbManager
    {
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

            _filename = GetFullDbFilename(_currentDatacenterParameters.AggregateId);
            _logFile.AppendLine($@"Db full filename: {_filename}");

            _connectionString = $@"Data Source={_filename}; Version=3;";
            _logFile.AppendLine($@"Connection string: <<{_connectionString}>>");

            CreateIfNeeded();
        }

        private string GetFullDbFilename(Guid aggregateId)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            _logFile.AppendLine($@"Application path: {appPath}");

            return FileOperations.GetParentFolder(appPath) +
                   $@"\Cache\GraphDb\{_serverAddress}\{aggregateId.ToString()}.sqlite3";
        }

        public async Task SaveEvents(string[] jsons)
        {
            try
            {
                using (var dbContext = new LocalDbSqliteContext(_connectionString))
                {
                    foreach (var json in jsons)
                    {
                        dbContext.EsEvents.Add(new EsEvent() { Json = json });
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"SaveEvents : {e.Message}");
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
                            .Where(e => e.Id > lastEventInSnapshot)
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
                        return new byte[0];
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
                return 1;
            }
        }

        public void CreateIfNeeded()
        {
            var s = AppDomain.CurrentDomain.BaseDirectory + $@"..\Cache\GraphDb\{_serverAddress}";
            if (!Directory.Exists(s))
                Directory.CreateDirectory(s);

            if (!File.Exists(_filename))
                InitializeLocalBase();
        }

        private void InitializeLocalBase()
        {
            SQLiteConnection.CreateFile(_filename);
            using (SQLiteConnection conn = new SQLiteConnection($@"Data Source={_filename}; Version=3;"))
            {
                try
                {
                    conn.Open();

                    const string sql = @"CREATE TABLE EsEvents (Id INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, Json TEXT)";
                    SQLiteCommand command = new SQLiteCommand(sql, conn);
                    command.ExecuteNonQuery();

                    const string sql2 = "CREATE TABLE EsSnapshots (Id INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, LastIncludedEvent INTEGER, Snapshot	BLOB)";
                    SQLiteCommand command2 = new SQLiteCommand(sql2, conn);
                    command2.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    _logFile.AppendLine($@"InitializeLocalBase: {ex.Message}");
                }

                if (conn.State == ConnectionState.Open)
                {
                    _logFile.AppendLine(@"Local cache created successfully");
                }
            }
        }
    }
}