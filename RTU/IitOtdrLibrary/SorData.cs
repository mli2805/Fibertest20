﻿using System;
using System.IO;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.IO;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public static class SorData
    {
        public static string TryGetFromBytes(byte[] buffer, out OtdrDataKnownBlocks otdrDataKnownBlocks)
        {
            using (var stream = new MemoryStream(buffer))
            {
                try
                {
                    otdrDataKnownBlocks = new OtdrDataKnownBlocks(new OtdrReader(stream).Data);
                    return "";
                }
                catch (Exception e)
                {
                    otdrDataKnownBlocks = null;
                    return e.Message;
                }
            }
        }

        public static OtdrDataKnownBlocks FromBytes(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return new OtdrDataKnownBlocks(new OtdrReader(stream).Data);
            }
        }

        public static byte[] ToBytes(this OtdrDataKnownBlocks sorData)
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
        public static void Save(byte[] sorBytes, string filename)
        {
            if (sorBytes == null)
                return;

            if (File.Exists(filename))
                File.Delete(filename);
            using (FileStream fs = File.Create(filename))
            {
                fs.Write(sorBytes, 0, sorBytes.Length);
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

        private static double GetOwtToMmCoeff(this OtdrDataKnownBlocks sorData)
        {
            return LightSpeed * 100000 / sorData.FixedParameters.RefractionIndex;
        }

        public static double GetTraceLengthKm(this OtdrDataKnownBlocks sorData)
        {
            var owt = sorData.KeyEvents.KeyEvents[sorData.KeyEvents.KeyEventsCount - 1].EventPropagationTime;
            return sorData.OwtToLenKm(owt);
        }

      
        public static int GetDistanceBetweenLandmarksInMm(
            this OtdrDataKnownBlocks sorData, int leftIndex, int rightIndex)
        {
            var owt1 = sorData.LinkParameters.LandmarkBlocks[leftIndex].Location;
            var owt2 = sorData.LinkParameters.LandmarkBlocks[rightIndex].Location;
            return (int) ((owt2 - owt1) * GetOwtToMmCoeff(sorData));
        }

        public static int GetOwtFromMm(this OtdrDataKnownBlocks sorData, int distance)
        {
            return (int) (distance / sorData.GetOwtToMmCoeff());
        }
    }
}