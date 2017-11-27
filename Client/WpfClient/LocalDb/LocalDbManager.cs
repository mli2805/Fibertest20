using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class LocalDbManager : ILocalDbManager
    {
        private readonly IMyLog _logFile;

        private string _serverAddress;
        private string _filename;
        private string _connectionString;

        public LocalDbManager(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void Initialize(string serverAddress)
        {
            _serverAddress = serverAddress;
            _filename = $@"..\Db\{serverAddress}\localGraph.sqlite3";
            _connectionString = $@"Data Source={_filename}; Version=3;";
        }

        public void SaveEvents(string[] jsons)
        {
            try
            {
                using (var dbContext = new LocalDbSqliteContext(_connectionString))
                {
                    foreach (var json in jsons)
                    {
                        dbContext.EsEvents.Add(new EsEvent() { Json = json });
                    }
                    dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"SaveEvents : {e.Message}");
            }
        }


        public string[] LoadEvents()
        {
            try
            {
                CreateIfNeeded();
                using (var dbContext = new LocalDbSqliteContext(_connectionString))
                {
                    var jsons = dbContext.EsEvents.Select(j => j.Json).ToArray();
                    return jsons;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"SaveEvents : {e.Message}");
                return new string[0];
            }
        }

        private void CreateIfNeeded()
        {
            if (!Directory.Exists(@"..\Db"))
                Directory.CreateDirectory(@"..\Db");
            if (!Directory.Exists($@"..\Db\{_serverAddress}"))
                Directory.CreateDirectory($@"..\Db\{_serverAddress}");
            if (!File.Exists(_filename))
            {
                InitializeLocalBase();
            }
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
                    _logFile.AppendLine($@"Local db opened successfully");
                }
            }
        }
    }
}