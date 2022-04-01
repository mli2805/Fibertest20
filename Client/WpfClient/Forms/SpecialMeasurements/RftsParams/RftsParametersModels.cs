using System.Collections.Generic;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RftsParametersModel
    {
        public List<RftsParamsLevelModel> Levels { get; set; }
        public List<RftsUniParamModel> UniParams { get; set; }
    }

    public class RftsParamsLevelModel
    {
        public string LevelName { get; set; }
        public bool IsEnabled { get; set; }
        public List<ThresholdLine> Lines { get; set; }

        // for backward conversion
        public string Code;
    }

    public class ThresholdLine
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public bool IsRelative { get; set; }
        public string IsRelString => IsRelative ? Resources.SID__rel__ : Resources.SID__abs__;
        public double Value { get; set; }
    }
  
    public class RftsUniParamModel
    {
        public string Name { get; set; }
        public double Value { get; set; }

        // for backward conversion
        public string Code;
    }
}
