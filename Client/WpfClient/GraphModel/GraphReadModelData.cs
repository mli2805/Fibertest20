using System.Collections.ObjectModel;

namespace Iit.Fibertest.Client
{
    public class GraphReadModelData
    {
        public ObservableCollection<NodeVm> Nodes { get; set; }
        public ObservableCollection<FiberVm> Fibers { get; set; }
    }
}