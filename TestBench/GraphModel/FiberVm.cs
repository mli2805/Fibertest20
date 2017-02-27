using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class FiberVm : PropertyChangedBase
    {
        private NodeVm _node1;
        private NodeVm _node2;
        public Guid Id { get; set; }

        public NodeVm Node1
        {
            get { return _node1; }
            set
            {
                if (Equals(value, _node1)) return;
                _node1 = value;
                NotifyOfPropertyChange();
            }
        }

        public NodeVm Node2
        {
            get { return _node2; }
            set { _node2 = value; }
        }

        // if empty fiber is not in any trace
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

        public FiberState State => States.Count == 0 ? FiberState.NotInTrace : States.Values.Max();

        public int UserInputedLength { get; set; }
    }
}