using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Iit.Fibertest.UtilsLib;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Iit.Fibertest.Graph
{
    public static class ModelJsonSerializer
    {
        public static Task<byte[]> SerializeAsJson(this Model model, IMyLog logFile)
        {
            try
            {
                var json = JsonConvert.SerializeObject(model);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                using (var memoryStream = new MemoryStream())
                {
                    using (var gz = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
                    {
                        gz.Write(bytes, 0, bytes.Length);

                    }

                    return Task.FromResult(memoryStream.ToArray());
                }
            }
            catch (Exception e)
            {
                logFile.AppendLine(@"Model JSON serialization: " + e.Message);
                return null;
            }
        }

        public static async Task<bool> DeserializeJson(this Model model, IMyLog logFile, byte[] compressed)
        {
            try
            {
                using (var inMs = new MemoryStream(compressed))
                using (var gz = new GZipStream(inMs, CompressionMode.Decompress))
                using (var outMs = new MemoryStream())
                {
                    await gz.CopyToAsync(outMs);
                    byte[] unpacked = outMs.ToArray();
                    var json = Encoding.UTF8.GetString(unpacked);
                    var model2 = JsonConvert.DeserializeObject<Model>(json);
                    model.CopyFrom(model2);
                    return true;
                }
            }
            catch (Exception e)
            {
                logFile.AppendLine(@"Model JSON deserialization: " + e.Message);
                return false;
            }
        }
    }
}
