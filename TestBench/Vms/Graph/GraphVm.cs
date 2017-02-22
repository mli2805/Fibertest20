using System.Collections.ObjectModel;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.TestBench
{
    public partial class GraphVm : PropertyChangedBase
    {
        public ObservableCollection<NodeVm> Nodes { get; }
        public ObservableCollection<FiberVm> Fibers { get; }
        public ObservableCollection<RtuVm> Rtus { get; }
        public ObservableCollection<EquipmentVm> Equipments { get; }
        public ObservableCollection<TraceVm> Traces { get; }

        private string _currentMousePosition;
        public string CurrentMousePosition
        {
            get { return _currentMousePosition; }
            set
            {
                if (value.Equals(_currentMousePosition)) return;
                _currentMousePosition = value;
                NotifyOfPropertyChange();
            }
        }

        private object _request;
        public object Request
        {
            get { return _request; }
            set
            {
                if (Equals(value, _request)) return;
                _request = value;
                NotifyOfPropertyChange();
            }
        }

        public GraphVm()
        {
            Nodes = new ObservableCollection<NodeVm>();
            Fibers = new ObservableCollection<FiberVm>();
            Rtus = new ObservableCollection<RtuVm>();
            Equipments = new ObservableCollection<EquipmentVm>();
            Traces = new ObservableCollection<TraceVm>();

            IsEquipmentVisible = true;
        }

        public void Apply(RtuAtGpsLocationAdded evnt)
        {
            var nodeVm = new NodeVm()
            {
                Id = evnt.NodeId,
                State = FiberState.Ok,
                Type = EquipmentType.Rtu,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude)
            };
            Nodes.Add(nodeVm);

            var rtuVm = new RtuVm() { Id = evnt.Id, Node = nodeVm };
            Rtus.Add(rtuVm);
        }

    }
}