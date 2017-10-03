using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.Dto
{
    public class MoniResult
    {
        public bool IsNoFiber { get; set; }
        public bool IsFiberBreak { get; set; }

        public List<MoniLevel> Levels { get; set; } = new List<MoniLevel>();

        public BaseRefType BaseRefType { get; set; }
        public double FirstBreakDistance { get; set; }

        public byte[] SorBytes { get; set; }

        public FiberState GetAggregatedResult()
        {
            if (IsNoFiber)
                return FiberState.NoFiber;
            if (IsFiberBreak)
                return FiberState.FiberBreak;

            var lvl = Levels.LastOrDefault(l => l.IsLevelFailed);
            return lvl == null ? FiberState.Ok : (FiberState) (int) lvl.Type;
        }
    }
}
