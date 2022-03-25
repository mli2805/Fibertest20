using System.Collections.Generic;

namespace Iit.Fibertest.Client
{
    public class RftsParams
    {
        public int LevelNumber;
        public readonly List<RftsParamsLevel> Levels = new List<RftsParamsLevel>();
        public int UniversalParamNumber;
        public List<RftsUniParameter> UniParams = new List<RftsUniParameter>();
    }
    
    public class RftsParamsLevel
    {
        public string LevelName;
        public bool Enabled; // stored as 0 or 1
        public RftsLevelThresholdSet LevelThresholdSet;
        public Threshold Eelt;
    }

    public class RftsLevelThresholdSet
    {
        public Threshold Lt;
        public Threshold Rt;
        public Threshold Ct;
    }

    public class Threshold
    {
        public bool Absolute; // stored as 0 or 1
        public int AbsoluteThreshold;
        public int RelativeThreshold;
    }

    public class RftsUniParameter
    {
        public string Name;
        public int Value;
        public int Scale;
    }
}
