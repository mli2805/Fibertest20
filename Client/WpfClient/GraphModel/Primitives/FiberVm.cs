﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class FiberVm : PropertyChangedBase
    {
        public Guid Id { get; set; }

        private NodeVm _node1;
        public NodeVm Node1
        {
            get => _node1;
            set
            {
                if (Equals(value, _node1)) return;
                _node1 = value;
                NotifyOfPropertyChange();
            }
        }

        public NodeVm Node2 { get; set; }


        // if empty - fiber is not in any trace
        public Dictionary<Guid, FiberState> States { get; set; } = new Dictionary<Guid, FiberState>();

        public void SetState(Guid traceId, FiberState traceState)
        {
            States[traceId] = traceState;

            NotifyOfPropertyChange(nameof(State));
        }

        public void RemoveState(Guid traceId)
        {
            if (States.ContainsKey(traceId))
                States.Remove(traceId);

            NotifyOfPropertyChange(nameof(State));
        }

        public List<Guid> HighLights { get; set; } = new List<Guid>();

        public void SetLightOnOff(Guid traceId, bool light)
        {
            if (HighLights == null) HighLights = new List<Guid>();
            if (light && !HighLights.Contains(traceId))
            {
                HighLights.Add(traceId);
                if (HighLights.Count == 1)
                    NotifyOfPropertyChange(nameof(State));
            }

            if (!light && HighLights.Contains(traceId))
            {
                HighLights.Remove(traceId);
                if (HighLights.Count == 0)
                    NotifyOfPropertyChange(nameof(State));
            }
        }

        public void ClearLight()
        {
            HighLights.Clear();
            NotifyOfPropertyChange(nameof(State));
        }

        public FiberState State => HighLights.Any()
           ? FiberState.HighLighted
           : TracesWithExceededLossCoeff.Any()
                ? TracesWithExceededLossCoeff.Values.Max()
                : States.Count == 0
                    ? FiberState.NotInTrace
                    : States.Values.Max();

        public Dictionary<Guid, FiberState> TracesWithExceededLossCoeff { get; set; } = new Dictionary<Guid, FiberState>();

        public void SetBadSegment(Guid traceId, FiberState lossCoeffSeriousness)
        {
            if (TracesWithExceededLossCoeff.ContainsKey(traceId))
                TracesWithExceededLossCoeff[traceId] = lossCoeffSeriousness;
            else
            {
                TracesWithExceededLossCoeff.Add(traceId, lossCoeffSeriousness);
                if (TracesWithExceededLossCoeff.Count == 1)
                    NotifyOfPropertyChange(nameof(IsBadSegment));
            }

            NotifyOfPropertyChange(nameof(State));
        }

        public void RemoveBadSegment(Guid traceId)
        {
            if (TracesWithExceededLossCoeff.ContainsKey(traceId))
            {
                TracesWithExceededLossCoeff.Remove(traceId);

                NotifyOfPropertyChange(nameof(IsBadSegment));
                NotifyOfPropertyChange(nameof(State));
            }
        }

        public bool IsBadSegment => TracesWithExceededLossCoeff.Any();
    }
}