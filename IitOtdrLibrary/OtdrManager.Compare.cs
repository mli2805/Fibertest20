using System.Collections.Generic;
using System.IO;
using System.Linq;
using Optixsoft.SharedCommons.SorSerialization;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.IO;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace IitOtdrLibrary
{
    public partial class OtdrManager
    {
        private MoniLevelType GetMoniLevelType(RftsLevelType level)
        {
            switch (level)
            {
                case RftsLevelType.Minor:
                    return MoniLevelType.Minor;
                case RftsLevelType.Major:
                    return MoniLevelType.Major;
                case RftsLevelType.Critical:
                    return MoniLevelType.Critical;
                default:
                    return MoniLevelType.User;
            }
        }

        private void SetMoniResultFlags(MoniResult moniResult, ComparisonReturns returnCode)
        {
            switch (returnCode)
            {
                case ComparisonReturns.Ok:
                    break;
                case ComparisonReturns.FiberBreak:
                    moniResult.IsFiberBreak = true;
                    break;
                case ComparisonReturns.NoLink:
                    moniResult.IsNoFiber = true;
                    break;
                default:
                    _rtuLogger.AppendLine($"something goes wrong, code {returnCode}");
                    break;
            }
        }

        private static EmbeddedData BaseBufferToEmbeddedData(byte[] buffer)
        {
            return new EmbeddedData
            {
                Description = "SOR",
                DataSize = buffer.Length,
                Data = buffer.ToArray()
            };
        }

        public MoniResult CompareMeasureWithBase(byte[] baseBuffer, byte[] measBuffer, bool includeBase)
        {
            var baseSorData = SorData.FromBytes(baseBuffer);
            var measSorData = SorData.FromBytes(measBuffer);
            measSorData.IitParameters.Parameters = baseSorData.IitParameters.Parameters;

            var embeddedData = new List<EmbeddedData>();
            if (includeBase)
                embeddedData.Add(BaseBufferToEmbeddedData(baseBuffer));

            MoniResult moniResult = Compare(baseSorData, ref measSorData, embeddedData);

            measSorData.EmbeddedData.EmbeddedDataBlocks = embeddedData.ToArray();
            measSorData.EmbeddedData.EmbeddedBlocksCount = (ushort)embeddedData.Count;
            measSorData.Save(@"c:\temp\MeasWithBase.sor");
            return moniResult;
        }

        private MoniResult Compare(OtdrDataKnownBlocks baseSorData, ref OtdrDataKnownBlocks measSorData, List<EmbeddedData> embeddedData)
        {
            MoniResult moniResult = new MoniResult();

            var levelCount = baseSorData.RftsParameters.LevelsCount;
            _rtuLogger.AppendLine($"Comparison begin. Level count = {levelCount}");

            for (int i = 0; i < levelCount; i++)
            {
                var rftsLevel = baseSorData.RftsParameters.Levels[i];
                if (rftsLevel.IsEnabled)
                {
                    baseSorData.RftsParameters.ActiveLevelIndex = i;
                    CompareOneLevel(baseSorData, ref measSorData, GetMoniLevelType(rftsLevel.LevelName), moniResult);
                    embeddedData.Add(RftsEventsToEmbeddedData(measSorData));
                }
            }
            return moniResult;
        }


        private EmbeddedData RftsEventsToEmbeddedData(OtdrDataKnownBlocks sorData)
        {
            byte[] rftsEventsBytes = RftsEventsToBytes(sorData);
            return new EmbeddedData()
            {
                Description = "RFTSEVENTS",
                BlockId = "",
                Comment = "",
                DataSize = rftsEventsBytes.Length,
                Data = rftsEventsBytes
            };
        }
        private byte[] RftsEventsToBytes(OtdrDataKnownBlocks sorData)
        {
            sorData.GeneralParameters.Language = LanguageCode.Utf8;
            using (MemoryStream ms = new MemoryStream())
            {
                System.IO.BinaryWriter w = new System.IO.BinaryWriter(ms);
                OpxSerializer opxSerializer = new OpxSerializer(
                    new Optixsoft.SharedCommons.SorSerialization.BinaryWriter(
                        w, sorData.GeneralParameters.Language.GetEncoding()), 
                        new FixDistancesContext(sorData.FixedParameters));

                var otdrBlock = new OtdrBlock(sorData.RftsEvents);
                var list = new List<OtdrBlock>() {otdrBlock};
                list.UpdateBlocks(otdrBlock.RevisionNumber);

                w.Write((ushort)otdrBlock.RevisionNumber);
                opxSerializer.Serialize(otdrBlock.Body, otdrBlock.RevisionNumber);
                return ms.ToArray();
            }
        }

        private void CompareOneLevel(OtdrDataKnownBlocks baseSorData, ref OtdrDataKnownBlocks measSorData,  MoniLevelType type, MoniResult moniResult)
        {
            var moniLevel = new MoniLevel {Type = type};

            // allocate memory
            var baseIntPtr = IitOtdr.SetSorData(SorData.ToBytes(baseSorData));
            IitOtdr.SetBaseForComparison(baseIntPtr);

            // allocate memory
            var measIntPtr = IitOtdr.SetSorData(SorData.ToBytes(measSorData));
            var returnCode = IitOtdr.CompareActiveLevel(measIntPtr);

            var size = IitOtdr.GetSorDataSize(measIntPtr);
            byte[] buffer = new byte[size];
            IitOtdr.GetSordata(measIntPtr, buffer, size);
            measSorData = SorData.FromBytes(buffer);

            moniLevel.IsLevelFailed = (measSorData.RftsEvents.Results & MonitoringResults.IsFailed) != 0;
            moniResult.Levels.Add(moniLevel);

            var levelResult = returnCode != ComparisonReturns.Ok ? returnCode.ToString() : moniLevel.IsLevelFailed ? "Failed!" : "OK!";
            _rtuLogger.AppendLine($"Level {type} comparison result = {levelResult}!");

            SetMoniResultFlags(moniResult, returnCode);

            // free memory
            IitOtdr.FreeSorDataMemory(measIntPtr);
            IitOtdr.FreeSorDataMemory(baseIntPtr);
        }
    }
}
