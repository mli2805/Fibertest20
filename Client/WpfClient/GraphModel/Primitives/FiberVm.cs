using System;
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
        private bool _isHighlighted;

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
            if (States.ContainsKey(traceId))
                States[traceId] = traceState;
            else
                States.Add(traceId, traceState);

            NotifyOfPropertyChange(nameof(State));
        }

        public void RemoveState(Guid traceId)
        {
            if (States.ContainsKey(traceId))
                States.Remove(traceId);

            NotifyOfPropertyChange(nameof(State));
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (value == _isHighlighted) return;
                _isHighlighted = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(State));
            }
        }

        public FiberState State => IsHighlighted 
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
                TracesWithExceededLossCoeff.Add(traceId, lossCoeffSeriousness);

            NotifyOfPropertyChange(nameof(IsBadSegment));
            NotifyOfPropertyChange(nameof(State));
        }

        public void RemoveBadSegment(Guid traceId)
        {
            if (TracesWithExceededLossCoeff.ContainsKey(traceId))
                TracesWithExceededLossCoeff.Remove(traceId);

            NotifyOfPropertyChange(nameof(IsBadSegment));
            NotifyOfPropertyChange(nameof(State));
        }

        public bool IsBadSegment => TracesWithExceededLossCoeff.Any();
    }
}