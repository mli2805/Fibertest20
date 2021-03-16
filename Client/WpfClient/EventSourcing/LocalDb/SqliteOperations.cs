using System.Data;
using System.Data.SQLite;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public static class SqliteOperations
    {
        public static void CreateCacheTables(string filename, IMyLog logFile)
        {
            using (SQLiteConnection conn = new SQLiteConnection($@"Data Source={filename}; Version=3;"))
            {
                try
                {
                    conn.Open();

                    const string sql = @"CREATE TABLE IF NOT EXISTS EsEvents (Id INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, EventId INTEGER UNIQUE, Json TEXT)";
                    SQLiteCommand command = new SQLiteCommand(sql, conn);
                    command.ExecuteNonQuery();

                    const string sql2 =
                        "CREATE TABLE IF NOT EXISTS EsSnapshots (Id INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, LastIncludedEvent INTEGER, Snapshot	BLOB)";
                    SQLiteCommand command2 = new SQLiteCommand(sql2, conn);
                    command2.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    logFile.AppendLine($@"CreateCacheTables: {ex.Message}");
                }

                if (conn.State == ConnectionState.Open)
                {
                    logFile.AppendLine(@"Local cache created successfully");
                }
            }
        }

        public static void DropCacheTables(string filename, IMyLog logFile)
        {
            using (SQLiteConnection conn = new SQLiteConnection($@"Data Source={filename}; Version=3;"))
            {
                try
                {
                    conn.Open();

                    const string sql = @"DROP TABLE IF EXISTS EsEvents";
                    SQLiteCommand command = new SQLiteCommand(sql, conn);
                    command.ExecuteNonQuery();

                    const string sql2 = "DROP TABLE IF EXISTS EsSnapshots";
                    SQLiteCommand command2 = new SQLiteCommand(sql2, conn);
                    command2.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    logFile.AppendLine($@"DropCacheTables: {ex.Message}");
                }

                if (conn.State == ConnectionState.Open)
                {
                    logFile.AppendLine(@"Local cache dropped successfully");
                }
            }
        }
    }
}