using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class Fiber
    {
        public Guid FiberId { get; set; }
        public Guid NodeId1 { get; set; }
        public Guid NodeId2 { get; set; }

        public double UserInputedLength { get; set; }
        public double OpticalLength { get; set; }

        // if empty - fiber is not in any trace; pair contains trace.TraceId : trace.TraceState
        public Dictionary<Guid, FiberState> States { get; set; } = new Dictionary<Guid, FiberState>();

        public void SetState(Guid traceId, FiberState traceState)
        {
            States[traceId] = traceState;
        }

        public void RemoveState(Guid traceId)
        {
            if (States.ContainsKey(traceId))
                States.Remove(traceId);
        }

        public List<Guid> HighLights { get; set; } = new List<Guid>();

        public void SetLightOnOff(Guid traceId, bool light)
        {
            if (HighLights == null) HighLights = new List<Guid>();
            if (light && !HighLights.Contains(traceId))
            {
                HighLights.Add(traceId);
            }

            if (!light && HighLights.Contains(traceId))
            {
                HighLights.Remove(traceId);
            }
        }

        public Dictionary<Guid, FiberState> TracesWithExceededLossCoeff { get; set; } = new Dictionary<Guid, FiberState>();

        public void SetBadSegment(Guid traceId, FiberState lossCoeffSeriousness)
        {
            TracesWithExceededLossCoeff[traceId] = lossCoeffSeriousness;
        }

        public void RemoveBadSegment(Guid traceId)
        {
            if (TracesWithExceededLossCoeff.ContainsKey(traceId))
                TracesWithExceededLossCoeff.Remove(traceId);
        }

    }
}