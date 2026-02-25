using System.Collections.Generic;
using System.Globalization;

namespace Iit.Fibertest.Client
{
    public class RftsParams
    {
        public int LevelNumber;
        public List<RftsParamsLevel> Levels { get; set; } = new List<RftsParamsLevel>();
        public int UniversalParamNumber;
        public List<RftsUniParameter> UniParams { get; set; } = new List<RftsUniParameter>();
    }

    public class RftsParamsLevel
    {
        public string LevelName { get; set; }
        public bool Enabled { get; set; }
        public RftsLevelThresholdSet LevelThresholdSet { get; set; }
        public Threshold Eelt { get; set; }
    }

    public class RftsLevelThresholdSet
    {
        public Threshold Lt { get; set; }
        public Threshold Rt { get; set; }
        public Threshold Ct { get; set; }
    }

    public class Threshold
    {
        public bool Absolute { get; set; }
        public int AbsoluteThreshold { get; set; }
        public int RelativeThreshold { get; set; }
    }

    public class RftsUniParameter
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public int Scale { get; set; }
        public string Comment { get; set; }

        public override string ToString()
        {
            return ((double)Value / Scale).ToString(CultureInfo.InvariantCulture);
        }

        public void Set(double value)
        {
            Value = (int)(value * 10000);
            Scale = 10000;
        }
    }
}