using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using MySql.Data.MySqlClient;

namespace Iit.Fibertest.DbMigrator
{
    public class SorFetcher
    {
        private readonly MySqlConnection _conn;
        public SorFetcher(string serverIp)
        {
            string mySqlConnectionString = $"server={serverIp};port=3306;user id=root;password=root;database=fibertest";
            _conn = new MySqlConnection(mySqlConnectionString);
        }

        public AssignBaseRefsDto GetAssignBaseRefsDto(int oldTraceId, Guid traceGuid, Guid rtuGuid)
        {
            var cmd = new AssignBaseRefsDto()
            {
                RtuId = rtuGuid,
                TraceId = traceGuid,
                OtauPortDto = null,
                BaseRefs = GetBaseRefList(oldTraceId),
                DeleteOldSorFileIds = new List<int>(),
            };
            return cmd;
        }

        private List<BaseRefDto> GetBaseRefList(int oldTraceId)
        {
            var result = new List<BaseRefDto>();
            var records = GetTraceBaseRecords(oldTraceId);
            foreach (var record in records)
            {
                var baseRefDto = new BaseRefDto()
                {
                    Id = Guid.NewGuid(),
                    UserName = "migrator",
                    SaveTimestamp = record.BaseTimestamp,
                    Duration = TimeSpan.Zero, // ?
                    BaseRefType = record.BaseRefType,
                    SorFileId = -1, // ?
                    SorBytes = GetMeasBytes(record.FileId),
                };
                result.Add(baseRefDto);
            }
            return result;
        }

        private List<TraceBaseRecord> GetTraceBaseRecords(int oldTraceId)
        {
            _conn.Open();
            string cmdString = "select * from tracebase where fileId <> 1 AND traceId = " + oldTraceId;
            MySqlCommand cmd = new MySqlCommand(cmdString, _conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            var records = new List<TraceBaseRecord>();
            while (reader.Read())
            {
                var record = new TraceBaseRecord();
                record.TraceId = (int)reader[1];
                record.BaseRefType = (int)reader[2] == 1 ? BaseRefType.Precise : BaseRefType.Fast; // уточнить
//                var str = reader[4].ToString();
//                record.BaseTimestamp = DateTime.Parse(str);
                record.BaseTimestamp = (DateTime)reader[4];
                record.FileId = (int)(uint)reader[5];

                records.Add(record);
            }
            reader.Close();
            _conn.Close();
            return records;
        }

        private byte[] GetMeasBytes(int fileId)
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
    }
}