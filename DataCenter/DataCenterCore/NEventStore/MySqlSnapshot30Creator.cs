using System;
using System.Data;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Iit.Fibertest.DataCenterCore
{

    public static class Snapshot30TableCreator
    {
        public static bool CheckIfSnapshots30Exists(string connectionString)
        {
            var sql = @"
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'ft20efcore' 
AND TABLE_NAME = 'snapshots30';
";
            using (var conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;

                conn.Open();
                var result = cmd.ExecuteScalar();
                conn.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // не always! если нет строки, то возвращает null;
                return result != null;
            }
        }

        public static void CreateSnapshot30Table(string connectionString)
        {
            var sql = @"
   CREATE TABLE ft20efcore.snapshots30 (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    StreamIdOriginal char(36) NOT NULL,
    LastEventNumber INT NOT NULL,
    LastEventDate DATETIME NOT NULL,
    Payload LONGBLOB,
    PayloadLength BIGINT NOT NULL DEFAULT 0
);
";

            using (var conn = new MySqlConnection(connectionString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}
