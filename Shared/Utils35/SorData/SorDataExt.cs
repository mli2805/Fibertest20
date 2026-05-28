using System.Collections.Generic;
using System.Linq;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.UtilsLib
{
    public static class SorDataExt
    {
        public static int GetLandmarkIndexForKeyEventIndex(this OtdrDataKnownBlocks sorData, int keyEventIndex)
        {
            var keyEventNumber = keyEventIndex + 1;
            for (int i = 0; i < sorData.LinkParameters.LandmarksCount; i++)
            {
                if (sorData.LinkParameters.LandmarkBlocks[i].RelatedEventNumber == keyEventNumber)
                    return i;
            }

            return -1;
        }

        public static int GetLandmarkToTheLeftFromOwt(this OtdrDataKnownBlocks sorData, int owt)
        {
            var leftLandmarkIndex = 0;
            for (int i = 1; i < sorData.LinkParameters.LandmarksCount; i++)
            {
                if (sorData.LinkParameters.LandmarkBlocks[i].Location < owt)
                    leftLandmarkIndex = i;
                else return leftLandmarkIndex;
            }

            return leftLandmarkIndex; // owt to the right of end
        }

        public static double GetDeltaLen(this OtdrDataKnownBlocks sorData, char code)
        {
            //var param = code == 'R'
            //    ? sorData.RftsParameters.UniversalParameters.First(p => p.Name == "EvtRDetectDeltaLen")
            //    : sorData.RftsParameters.UniversalParameters.First(p => p.Name == "EvtDetectDeltaLen");

            //return (double)param.Value / param.Scale;
            var param = code == 'R'
                ? sorData.GetEvtRDetectDeltaLen()
                : sorData.GetEvtDetectDeltaLen();

            return param;
        }

        // R - с отражением
        private static double GetEvtRDetectDeltaLen(this OtdrDataKnownBlocks sorData)
        {
            var param = sorData.RftsParameters.UniversalParameters
                .FirstOrDefault(p => p.Name == "EvtRDetectDeltaLen");

            if (param != null)
                return (double)param.Value / param.Scale;

            return sorData.LenDs() * 3;
        }

        private static double LenDs(this OtdrDataKnownBlocks sorData)
        {
            return sorData.OwtToLen(sorData.GetOwtDs());
        }

        private static double OwtToLen(this OtdrDataKnownBlocks sorData, double owt)
        {
            return SorMathOwtToLen(owt, sorData.FixedParameters.RefractionIndex);
        }

        private static double GetOwtDs(this OtdrDataKnownBlocks sorData, int num = 0)
        {
            return sorData.FixedParameters.DataSpacing[num];
        } 

        // без отражения
        private static double GetEvtDetectDeltaLen(this OtdrDataKnownBlocks sorData)
        {
            var param = sorData.RftsParameters.UniversalParameters
                .FirstOrDefault(p => p.Name == "EvtDetectDeltaLen");

            if (param != null)
                return (double)param.Value / param.Scale;

            return SorMathNsToLen(sorData.GetRealNsPulse(), sorData.FixedParameters.RefractionIndex) / 3;
        }

        private static double SorMathNsToLen(double ns, double n)
        {
            return SorMathOwtToLen(SorMathNsToOwt(ns), n);
        }

        private static double SorMathNsToOwt(double ns)
        {
            const double owtNs = 0.2;
            return ns / owtNs;
        }

        private static double SorMathOwtToLen(double owt, double n)
        {
            const double lightSpeedKms = 299792.458;
            return lightSpeedKms / n * owt * 1e-10;
        }

        private static double GetRealNsPulse(this OtdrDataKnownBlocks sorData)
        {
           return sorData.FixedParameters.PulseWidths[0];
        }

        public static void EmbedBaseRef(this OtdrDataKnownBlocks measSorData, byte[] baseBytes)
        {

            if (measSorData.EmbeddedData.EmbeddedDataBlocks != null)
            {
                var embeddedData = measSorData.EmbeddedData.EmbeddedDataBlocks.ToList();
                embeddedData.Add(BufferToEmbeddedDataBlock(baseBytes));
                measSorData.EmbeddedData.EmbeddedDataBlocks = embeddedData.ToArray();
                measSorData.EmbeddedData.EmbeddedBlocksCount = (ushort)embeddedData.Count;
            }
            else
            {
                var embeddedData = new List<EmbeddedData> { BufferToEmbeddedDataBlock(baseBytes) };
                measSorData.EmbeddedData.EmbeddedDataBlocks = embeddedData.ToArray();
                measSorData.EmbeddedData.EmbeddedBlocksCount = (ushort)embeddedData.Count;

            }
        }

        private static EmbeddedData BufferToEmbeddedDataBlock(byte[] buffer)
        {
            return new EmbeddedData
            {
                Description = "SOR",
                DataSize = buffer.Length,
                Data = buffer.ToArray()
            };
        }
    }
}