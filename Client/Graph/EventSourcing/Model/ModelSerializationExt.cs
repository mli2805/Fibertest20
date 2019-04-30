using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public static class ModelSerializationExt
    {
        public static async Task<byte[]> Serialize(this Model model, IMyLog logFile)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    await Task.Delay(1);
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(stream, model);
                    var buf = stream.ToArray();
                    logFile.AppendLine($@"Model serialization: buffer size = {buf.Length:0,0.#}");
                    return buf;
                }
            }
            catch (Exception e)
            {
                logFile.AppendLine(@"Model serialization: " + e.Message);
                return null;
            }
        }

        public static async Task<Model> Deserialize(IMyLog logFile, byte[] buffer)
        {
            try
            {
                using (var stream = new MemoryStream(buffer))
                {
                    await Task.Delay(1);
                    var binaryFormatter = new BinaryFormatter();
                    var model = (Model)binaryFormatter.Deserialize(stream);
                    logFile.AppendLine(@"Model deserialized successfully!");
                    return model;
                }
            }
            catch (Exception e)
            {
                logFile.AppendLine(@"Model deserialization: " + e.Message);
                return null;
            }
        }
    }
}