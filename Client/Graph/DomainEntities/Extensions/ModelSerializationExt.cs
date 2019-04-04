using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public static class ModelSerializationExt
    {
        public static async Task<bool> Serialize(this Model model, IMyLog logFile)
        {
            try
            {
                using (Stream stream = new MemoryStream())
                {
                    await Task.Delay(1);
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(stream, model);
                    logFile.AppendLine($@"Model serialization: buffer size = {stream.Length}");
                    return true;
                }
            }
            catch (Exception e)
            {
                logFile.AppendLine(@"Model serialization: " + e.Message);
                return false;
            }
        }
    }
}