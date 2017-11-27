using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{

    /// <summary>
    /// While install Fibertest Client - 
    ///     create in Sqlite Db Browser database in folder "data source =..\Db\localGraph.sqlite3"
    ///     (check connection string in App.config)
    ///     and create only table:
    ///         CREATE TABLE `EsEvents` ( `Id` INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, `Json` TEXT )
    /// </summary>


    public class LocalDbManager : ILocalDbManager
    {
        private readonly IMyLog _logFile;
        private const string Filename = @"..\Db\localGraph.sqlite3";

        public LocalDbManager(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void SaveEvents(string[] jsons)
        {
            try
            {
//                using (var dbContext = new LocalDbContext())
                using (var dbContext = new LocalSqliteContext($@"Data Source={Filename}; Version=3;"))
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


        public void CreateIfNeeded()
        {
            if (!Directory.Exists(@"..\Db"))
                Directory.CreateDirectory(@"..\Db");
            if (!File.Exists(Filename))
            {
                InitializeLocalBase();
            }
        }

        public string[] LoadEvents()
        {
            try
            {
                CreateIfNeeded();
//                using (var dbContext = new LocalDbContext())
                using (var dbContext = new LocalSqliteContext($@"Data Source={Filename}; Version=3;"))
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

        public void InitializeLocalBase()
        {
            SQLiteConnection.CreateFile(Filename);
            using (SQLiteConnection conn = new SQLiteConnection($@"Data Source={Filename}; Version=3;"))
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