using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuAutoBaseViewModel : Screen
    {
        private readonly Model _readModel;
        private List<Trace> _traces;

        public bool IsOpen { get; set; }

        public RtuAutoBaseViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(RtuLeaf rtuLeaf)
        {
            _traces = GetAttachedTraces(rtuLeaf).ToList();
            _traces.AddRange(rtuLeaf
                .ChildrenImpresario
                .Children
                .OfType<OtauLeaf>()
                .SelectMany(GetAttachedTraces));
        }

        private IEnumerable<Trace> GetAttachedTraces(IPortOwner portOwner)
        {
            return portOwner.ChildrenImpresario
                .Children
                .OfType<TraceLeaf>()
                .Where(t => t.PortNumber > 0)
                .Select(l => _readModel.Traces.First(r => r.TraceId == l.Id))
                .ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
        }

        public async void Start()
        {
            foreach (var trace in _traces)
            {
                
            }
        }

        public void Close()
        {
            TryClose();
        }
        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            callback(true);
        }
    }
}
