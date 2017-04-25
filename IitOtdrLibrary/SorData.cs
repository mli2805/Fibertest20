using System.IO;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.IO;

namespace Iit.Fibertest.IitOtdrLibrary
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

        private const double LightSpeed = 0.000299792458; // km/ns
        public static double OwtToLenKm(this OtdrDataKnownBlocks sorData, double owt)
        {
            var owt1 = owt - sorData.GeneralParameters.UserOffset;
            return owt1 * GetOwtToKmCoeff(sorData);
        }

        public static double GetOwtToKmCoeff(this OtdrDataKnownBlocks sorData)
        {
            return LightSpeed / sorData.FixedParameters.RefractionIndex / 10;
        }
    }
}