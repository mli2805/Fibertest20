using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Iit.Fibertest.Graph
{
    public class Db
    {
        private string filename = @"db.bin";

        public List<object> Events { get; private set; } = new List<object>();

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

        public void Load()
        {
            try
            {
                using (Stream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    Events = (List<object>) binaryFormatter.Deserialize(fStream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}