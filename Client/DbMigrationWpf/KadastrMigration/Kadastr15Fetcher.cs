using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Iit.Fibertest.Graph;
using MySql.Data.MySqlClient;

namespace DbMigrationWpf
{
    public class Kadastr15Fetcher
    {
        private readonly GraphModel _graphModel;
        private readonly ObservableCollection<string> _progressLines;
        private readonly MySqlConnection _kadastr15;

        public Kadastr15Fetcher(string serverIp, int oldMySqlPort, GraphModel graphModel, ObservableCollection<string> progressLines)
        {
            _graphModel = graphModel;
            _progressLines = progressLines;
            string mySqlConnectionString = $"server={serverIp};port={oldMySqlPort};user id=root;password=root;database=kadastr";
            _kadastr15 = new MySqlConnection(mySqlConnectionString);
        }
        
        public KadastrModel Fetch()
        {
            var result = new KadastrModel();
            result.Wells = GetKadastrWells();
            if (result.Wells == null) return null;
            _progressLines.Add($"Fetched {result.Wells.Count} wells");
            result.Conpoints = GetKadastrConpoints();
            _progressLines.Add($"Fetched {result.Conpoints.Count} conpoints");
            return result;
        }

        private List<Conpoint> GetKadastrConpoints()
        {
            try
            {
                _kadastr15.Open();
                string cmdString = "select * from conpointsrelation";
                MySqlCommand cmd = new MySqlCommand(cmdString, _kadastr15);

                MySqlDataReader reader = cmd.ExecuteReader();
                var result = new List<Conpoint>();
                while (reader.Read())
                {
                    var conpoint20 = new Conpoint();
                    if (int.TryParse(reader["Id"].ToString(), out int id)) conpoint20.Id = id;
                    if (int.TryParse(reader["conpointId"].ToString(), out int wId)) conpoint20.InKadastrId = wId;
                  
                    result.Add(conpoint20);
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

        private List<Well> GetKadastrWells()
        {
            try
            {
                _kadastr15.Open();
                string cmdString = "select * from wellsrelation";
                MySqlCommand cmd = new MySqlCommand(cmdString, _kadastr15);

                MySqlDataReader reader = cmd.ExecuteReader();
                var result = new List<Well>();
                while (reader.Read())
                {
                    var well = new Well();
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