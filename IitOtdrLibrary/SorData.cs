using System.IO;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.IO;

namespace IitOtdrLibrary
{
    public static class SorData
    {
        public static OtdrDataKnownBlocks FromBytes(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return new OtdrDataKnownBlocks(new OtdrReader(stream).Data);
            }
        }

        public static byte[] ToBytes(OtdrDataKnownBlocks sorData)
        {
            using (var stream = new MemoryStream())
            {
                sorData.Save(stream);
                return stream.ToArray();
            }
        }

        public static void Save(this OtdrDataKnownBlocks sorData, string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
            using (FileStream fs = File.Create(filename))
            {
                sorData.Save(fs);
            }
        }
    }
}