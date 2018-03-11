using System.Collections.ObjectModel;

namespace Iit.Fibertest.Client
{
    public class GrmData
    {
        public ObservableCollection<NodeVm> Nodes { get; set; }
        public ObservableCollection<FiberVm> Fibers { get; set; }
        public ObservableCollection<TraceVm> Traces { get; set; }

    }
}