using System.Collections.Generic;

namespace Iit.Fibertest.Client
{
    public static class RftsParamsDefaults
    {
        public static RftsParams Create()
        {
            return new RftsParams()
            {
                LevelNumber = 3,
                Levels = { CreateRftsParamsLevel(0), CreateRftsParamsLevel(1), CreateRftsParamsLevel(2) },
                UniversalParamNumber = 16,
                UniParams = CreateUniParams()
            };
        }

        private static readonly string[] LevelNames = { @"minor", @"major", @"critical" };
        private static readonly int[,] RelativeThresholds = { {500, 10000, 100, 32500}, {700, 15000, 200, 32500}, {1000, 20000, 500, 32500} };

        private static RftsParamsLevel CreateRftsParamsLevel(int i)
        {
            return new RftsParamsLevel()
            {
                LevelName = LevelNames[i],
                Enabled = false,
                LevelThresholdSet = new RftsLevelThresholdSet()
                {
                    Lt = new Threshold { RelativeThreshold = RelativeThresholds[i, 0] },
                    Rt = new Threshold { RelativeThreshold = RelativeThresholds[i, 1] },
                    Ct = new Threshold { RelativeThreshold = RelativeThresholds[i, 2] }
                },
                Eelt = new Threshold() { RelativeThreshold = RelativeThresholds[i, 3] },
            };
        }

        private static List<RftsUniParameter> CreateUniParams()
        {
            return new List<RftsUniParameter>()
            {
                new RftsUniParameter(){ Name = @"EvtDetectDeltaCT", Value = 1, Scale = 10 },
                new RftsUniParameter(){ Name = @"EvtSearchStep", Value = 8, Scale = 1 },
                new RftsUniParameter(){ Name = @"EvtDetectDeltaLen", Value = 34, Scale = 10000 },
                new RftsUniParameter(){ Name = @"EvtRDetectDeltaLen", Value = 38, Scale = 10000 },
                new RftsUniParameter(){ Name = @"AutoLT", Value = 1, Scale = 10 },
                new RftsUniParameter(){ Name = @"AutoRT", Value = -65, Scale = 1 },
                new RftsUniParameter(){ Name = @"AutoET", Value = 6, Scale = 1 },
                new RftsUniParameter(){ Name = @"NoLinkDistance", Value = 5, Scale = 10 },
                new RftsUniParameter(){ Name = @"NoLinkDeltaDB", Value = 2, Scale = 1 },
                new RftsUniParameter(){ Name = @"EventRT", Value = 1, Scale = 1 },
                new RftsUniParameter(){ Name = @"EOFLMN", Value = -1, Scale = 1 },
                new RftsUniParameter(){ Name = @"CorrectMarkers", Value = 1, Scale = 1 },
                new RftsUniParameter(){ Name = @"EvtChangeLT", Value = 1, Scale = 1 },
                new RftsUniParameter(){ Name = @"EvtChangeRT", Value = 20, Scale = 1 },
                new RftsUniParameter(){ Name = @"EvtChangeCT", Value = 5, Scale = 10 },
                new RftsUniParameter(){ Name = @"EvtChangeET", Value = 2, Scale = 1 },
            };
        }
    }
}
