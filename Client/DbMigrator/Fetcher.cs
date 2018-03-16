using System;
using System.Collections.Generic;
using System.IO;
using Iit.Fibertest.Dto;
using MySql.Data.MySqlClient;

namespace Iit.Fibertest.DbMigrator
{
    public class Fetcher
    {
        private readonly string _mySqlConnectionString = $"server=172.16.4.115;port=3306;user id=root;password=root;database=fibertest";
        private readonly MySqlConnection _conn;
        public Fetcher()
        {
            _conn = new MySqlConnection(_mySqlConnectionString);
        }

        private List<TraceBaseRecord> GetTraceBaseRecords()
        {
            _conn.Open();
            string cmdString = "select * from tracebase where FileId <> 1";
            MySqlCommand cmd = new MySqlCommand(cmdString, _conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            var records = new List<TraceBaseRecord>();
            while (reader.Read())
            {
                var record = new TraceBaseRecord();
                record.TraceId = (int)reader[1];
                record.BaseRefType = (int)reader[2] == 1 ? BaseRefType.Precise : BaseRefType.Fast; // уточнить
                record.BaseTimestamp = (DateTime)reader[4];
                record.FileId = (int)(uint)reader[5];

                records.Add(record);
            }
            reader.Close();
            _conn.Close();
            return records;
        }

        private byte[] GetTraceBaseBytes(int fileId)
        {
            _conn.Open();
            string cmdString = "select filesize, uncompress(filebin) from measfiles where Id = " + fileId;
            MySqlCommand cmd = new MySqlCommand(cmdString, _conn);

            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            var filesize = (int)reader[0];
            var result = new byte[filesize];
            reader.GetBytes(1, 0, result, 0, filesize);

            reader.Close();
            _conn.Close();
            return result;
        }

        public void F()
        {
            var records = GetTraceBaseRecords();
            var sorBytes = GetTraceBaseBytes(records[0].FileId);
            File.WriteAllBytes(@"c:\temp\base.sor", sorBytes);
        }
    }

    public class TraceBaseRecord
    {
        public int TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public DateTime BaseTimestamp { get; set; }
        public int FileId { get; set; }
    }
}