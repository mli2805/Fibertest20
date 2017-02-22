using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.TextFormatting;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class FiberVm : PropertyChangedBase
    {
        public Guid Id { get; set; }
        public NodeVm Node1 { get; set; }
        public NodeVm Node2 { get; set; }

        // if empty fiber is not in any trace
        private Dictionary<Guid, FiberState> States { get; set; } = new Dictionary<Guid, FiberState>();

        public void SetState(Guid traceId, FiberState traceState)
        {
            if (States.ContainsKey(traceId))
                States[traceId] = traceState;
            else
                States.Add(traceId, traceState);

            NotifyOfPropertyChange(nameof(State));
        }

        public FiberState State => States.Count == 0 ? FiberState.NotInTrace : States.Values.Max();

        public int UserInputedLength { get; set; }
    }
}