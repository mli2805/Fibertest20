using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class LocalDbManager : ILocalDbManager
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private string _serverAddress;
        private string _filename;
        private string _connectionString;

        public LocalDbManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public void Initialize(Guid graphDbVersionOnServer)
        {
            var serverDoubleAddress = _iniFile.ReadDoubleAddress(11840);
            _serverAddress = serverDoubleAddress.Main.GetAddress();

            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            _logFile.AppendLine($@"Application path {appPath}");

            _filename = $@"..\Cache\GraphDb\{_serverAddress}\{graphDbVersionOnServer.ToString()}.sqlite3";

            var dbFullFilename = Path.Combine(appPath, _filename);
            _logFile.AppendLine($@"Db full filename {dbFullFilename}");

            _connectionString = $@"Data Source={dbFullFilename}; Version=3;";
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


        public async Task<string[]> LoadEvents()
        {
            try
            {
                CreateIfNeeded();

                // SQLite do not work asynchronously (event though there is a ToArrayAsync function)
                // https://stackoverflow.com/questions/42982444/entity-framework-core-sqlite-async-requests-are-actually-synchronous

                return await Task.Factory.StartNew(() =>
                {
                    using (var dataContext = new LocalDbSqliteContext(_connectionString))
                    {
                        return dataContext.EsEvents.Select(j => j.Json).ToArray();
                    }
                });

            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"SaveEvents : {e.Message}");
                return new string[0];
            }
        }

        private void CreateIfNeeded()
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
                    string sql = @"CREATE TABLE EsEvents (Id INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, Json TEXT)";

                    SQLiteCommand command = new SQLiteCommand(sql, conn);
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    _logFile.AppendLine($@"{ex.Message}");
                }

                if (conn.State == ConnectionState.Open)
                {
                    _logFile.AppendLine(@"Local cache created successfully");
                }
            }
        }
    }
}