using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;

namespace DbMigrationWpf
{
    public class KadastrMigrator
    {
        private readonly GraphModel _graphModel;
        private readonly ObservableCollection<string> _progressLines;
        private readonly MySqlConnection _kadastr15;
        public KadastrMigrator(string serverIp, GraphModel graphModel, ObservableCollection<string> progressLines)
        {
            _graphModel = graphModel;
            _progressLines = progressLines;
            string mySqlConnectionString = $"server={serverIp};port=3306;user id=root;password=root;database=kadastr";
            _kadastr15 = new MySqlConnection(mySqlConnectionString);
        }

        public void DoMigrate()
        {
            var wells = GetKadastrDb();
            _progressLines.Add($"Fetched {wells.Count} wells");
//            var conpoints = 
        }

        private List<Well20> GetKadastrDb()
        {
            try
            {
                _kadastr15.Open();
                string cmdString = "select * from wellsrelation";
                MySqlCommand cmd = new MySqlCommand(cmdString, _kadastr15);

                MySqlDataReader reader = cmd.ExecuteReader();
                var result = new List<Well20>();
                while (reader.Read())
                {
                    var well = new Well20();
                    if (int.TryParse(reader["Id"].ToString(), out int id)) well.Id = id;
                    if (int.TryParse(reader["wellId"].ToString(), out int wId)) well.InKadastrId = wId;
                    if (int.TryParse(reader["nodeId"].ToString(), out int nId))
                    {
                        _graphModel.NodesDictionary.TryGetValue(nId, out Guid fId);
                        well.InFibertestId = fId;
                    }
                    result.Add(well);
                }

                reader.Close();
                _kadastr15.Close();
                return result; }
            catch (Exception e)
            {
                _progressLines.Add(e.Message);
                return null;
            }
        }


    }
}