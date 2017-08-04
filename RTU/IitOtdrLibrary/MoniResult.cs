using System.Collections.Generic;
using Dto;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public class MoniResult
    {
        public bool IsNoFiber { get; set; }
        public bool IsFiberBreak { get; set; }
        public bool IsFailed { get; set; }

        public List<MoniLevel> Levels { get; set; } = new List<MoniLevel>();

        public ComparisonReturns Result { get; set; }

        public BaseRefType BaseRefType { get; set; }
        public double FirstBreakDistance { get; set; }

        public byte[] SorBytes { get; set; }
    }

    public class MoniLevel
    {
        public bool IsLevelFailed { get; set; }
        public MoniLevelType Type { get; set; }
    }
}
