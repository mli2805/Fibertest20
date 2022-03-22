using System.Collections.Generic;

namespace Iit.Fibertest.Client
{
    public class RftsParams
    {
        public int LevelNumber;
        public List<RftsLevel> Levels = new List<RftsLevel>();
        public int UniversalParamNumber;
        public List<RftsUniversalParameter> UniParams = new List<RftsUniversalParameter>();
    }
    
    public class RftsLevel
    {
        public string LevelName;
        public bool Enabled; // stored as 0 or 1
        public RftsLevelThresholdSet LevelThresholdSet;
        public ShortThreshold Eelt;
    }

    public class RftsLevelThresholdSet
    {
        public ShortThreshold Lt;
        public ShortThreshold Rt;
        public ShortThreshold Ct;
    }

    public class ShortThreshold
    {
        public bool Absolute; // stored as 0 or 1
        public int AbsoluteThreshold;
        public int RelativeThreshold;
    }

    public class RftsUniversalParameter
    {
        public string Name;
        public int Value;
        public int Scale;
    }
}
