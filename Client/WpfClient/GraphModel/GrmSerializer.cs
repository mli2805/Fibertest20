using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public static class GrmSerializer
    {
        public static byte[] Serialize(this GraphReadModel model, IMyLog logFile)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                try
                {
                    binaryFormatter.Serialize(memoryStream, model.Data);
                }
                catch (Exception e)
                {
                    logFile.AppendLine(@"GraphReadModel Serialize: " + e.Message);
                }
                return memoryStream.ToArray();
            }
        }

        public static void Deserialize(this GraphReadModel model, byte[] data, IMyLog logFile)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                try
                {
                    model.Data = (GrmData)binaryFormatter.Deserialize(memoryStream);
                }
                catch (Exception e)
                {
                    logFile.AppendLine(@"GraphReadModel Deserialize: " + e.Message);
                }
            }
        }
    }
}