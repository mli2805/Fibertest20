using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Iit.Fibertest.StringResources;
using Serilog;

namespace Iit.Fibertest.Graph
{
    public class Db
    {
        private readonly ILogger _log;
        private string filename = @"..\db\db.bin";

        public List<object> Events { get; private set; } = new List<object>();

        public Db(ILogger log)
        {
            _log = log;
            Load();
        }

        public void Add(object e)
        {
            Events.Add(e);
        }

        public void Save()
        {
            try
            {
                using (Stream fStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fStream, Events);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void Load()
        {
            try
            {
                using (Stream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    Events = (List<object>) binaryFormatter.Deserialize(fStream);
                }
                _log.Information(string.Format(Resources.SID_N_graph_events_loaded_successfully, Events.Count));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _log.Information(e.Message);
            }

        }
    }
}