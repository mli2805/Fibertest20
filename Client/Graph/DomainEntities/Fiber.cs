using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class Fiber
    {
        public Guid FiberId { get; set; }
        public Guid NodeId1 { get; set; }
        public Guid NodeId2 { get; set; }

        public double UserInputedLength { get; set; }
        public double OpticalLength { get; set; }

        // if empty - fiber is not in any trace
        public Dictionary<Guid, FiberState> States { get; set; } = new Dictionary<Guid, FiberState>();

        public void SetState(Guid traceId, FiberState traceState)
        {
            if (States.ContainsKey(traceId))
                States[traceId] = traceState;
            else
                States.Add(traceId, traceState);
        }

        public void RemoveState(Guid traceId)
        {
            if (States.ContainsKey(traceId))
                States.Remove(traceId);
        }

        public FiberState State => States.Count == 0 ? FiberState.NotInTrace : States.Values.Max();

        public List<Guid> TracesWithExceededLossCoeff { get; set; } = new List<Guid>();
        public bool IsBadSegment => TracesWithExceededLossCoeff.Any();

        public void AddBadSegment(Guid traceId)
        {
            if (TracesWithExceededLossCoeff.Contains(traceId)) return;

            TracesWithExceededLossCoeff.Add(traceId);
        }

        public void CleanBadSegment(Guid traceId)
        {
            if (!TracesWithExceededLossCoeff.Contains(traceId)) return;

            TracesWithExceededLossCoeff.Remove(traceId);
        }
    }
}