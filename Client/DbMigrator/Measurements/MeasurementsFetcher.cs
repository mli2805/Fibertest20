using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using MySql.Data.MySqlClient;

namespace Iit.Fibertest.DbMigrator
{
    public class MeasurementsFetcher
    {
        private readonly IMyLog _logFile;
        private readonly MySqlConnection _conn;
        public MeasurementsFetcher(string serverIp, IMyLog logFile)
        {
            _logFile = logFile;
            string mySqlConnectionString = $"server={serverIp};port=3306;user id=root;password=root;database=fibertest";
            _conn = new MySqlConnection(mySqlConnectionString);
        }

        public void TransferMeasurements(GraphModel graphModel, C2DWcfManager c2DWcfManager)
        {
            foreach (var pair in graphModel.TracesDictionary)
            {
                var rtuGuid = graphModel.AddTraceCommands.First(c => c.TraceId == pair.Value).RtuId;
                var listOfMeasurementForOneTrace = GetTraceMeasurementsList(pair.Key);
                Console.WriteLine($"{DateTime.Now}  {listOfMeasurementForOneTrace.Count} measurements for trace {pair.Key} found");


                var i = 0;
                List<AddMeasurementFromOldBase> list = new List<AddMeasurementFromOldBase>();
                foreach (var measurementRecord in listOfMeasurementForOneTrace)
                {
                    i++;
                    var dto = new AddMeasurementFromOldBase()
                    {
                        TraceId = pair.Value,
                        RtuId = rtuGuid,

                        BaseRefType = measurementRecord.BaseRefType,
                        MeasurementTimestamp = measurementRecord.MeasurementTimestamp,
                        TraceState = measurementRecord.TraceState,

                        SorBytes = SorFetcher.GetMeasBytes(_conn, measurementRecord.FileId),
                    };
                    list.Add(dto);

                    if (i % 100 == 0)
                    {
                        c2DWcfManager.SendMeas(list).Wait();
                        Console.WriteLine($"{DateTime.Now}  {i} measurements sent");
                        list = new List<AddMeasurementFromOldBase>();
                    }
                }
                c2DWcfManager.SendMeas(list).Wait();
                Console.WriteLine($"{DateTime.Now}  {i} measurements sent");
                _logFile.AppendLine($"{i} measurements for trace {pair.Value.First6()} are sent");
            }
        }

        private List<MeasurementRecord> GetTraceMeasurementsList(int oldTraceId)
        {
            _conn.Open();
            string cmdString = "select * from tracemeas where traceId = " + oldTraceId;
            MySqlCommand cmd = new MySqlCommand(cmdString, _conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            var records = new List<MeasurementRecord>();
            var i = 0;
            while (reader.Read())
            {
                try
                {
                    var record = new MeasurementRecord();
                    record.TraceId = (int)reader[3];
                    record.BaseRefType = (int)reader[4] == 1 ? BaseRefType.Precise : BaseRefType.Fast; // уточнить
                    record.MeasurementTimestamp = (DateTime)reader[6];
                    record.TraceState = (FiberState)reader[8];
                    record.FileId = (int)(uint)reader[11];

                    records.Add(record);
                    i++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _logFile.AppendLine($"Exception while converting {i}th measurement for {oldTraceId}");
                }
            }
            reader.Close();
            _conn.Close();
            return records;
        }
    }
}