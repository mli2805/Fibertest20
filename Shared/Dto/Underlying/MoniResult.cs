using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.Dto
{
    public class MoniResult
    {
        // Trace could be broken and ReturnCode could be MeasurementEndedNormally - means measurement process ended normally
        public ReturnCode ReturnCode;
     
        #region State of trace
        public bool IsNoFiber { get; set; }
        public bool IsFiberBreak { get; set; }
        public List<MoniLevel> Levels { get; set; } = new List<MoniLevel>();
        #endregion

        public BaseRefType BaseRefType { get; set; }
        public double FirstBreakDistance { get; set; }

        public List<AccidentInSor> Accidents { get; set; }
        public byte[] SorBytes { get; set; }

        public FiberState GetAggregatedResult()
        {
            if (ReturnCode == ReturnCode.MeasurementInterrupted)
                return FiberState.Unknown;

            if (IsNoFiber)
                return FiberState.NoFiber;
            if (IsFiberBreak)
                return FiberState.FiberBreak;

            var lvl = Levels.LastOrDefault(l => l.IsLevelFailed);
            return lvl == null ? FiberState.Ok : (FiberState) (int) lvl.Type;
        }

        public bool IsStateChanged(MoniResult previous)
        {
            if (previous == null) return true;
            var currentState = GetAggregatedResult();
            if (previous.GetAggregatedResult() != currentState)
                return true;

            if (currentState == FiberState.NoFiber || currentState == FiberState.Ok)
                return false;

            if (previous.Accidents.Count != Accidents.Count)
                return true;

            for (int i = 0; i < Accidents.Count; i++)
            {
                if (!Accidents[i].IsTheSame(previous.Accidents[i])) return true;
            }

            return false;
        }
    }
}
