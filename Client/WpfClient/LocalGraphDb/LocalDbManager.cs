using System;
using System.Data;
using System.Data.SQLite;
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

        public LocalDbManager(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void SaveEvents(string[] jsons)
        {
            try
            {
                using (var dbContext = new LocalDbContext())
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
                using (var dbContext = new LocalDbContext())
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
            using (SQLiteConnection conn = new SQLiteConnection(@"Data Source=..\Db\localGraph.sqlite3; Version=3;"))
            {
                try
                {
                    conn.Open();
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