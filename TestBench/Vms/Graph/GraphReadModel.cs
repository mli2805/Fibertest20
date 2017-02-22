using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.TestBench
{
    public partial class GraphReadModel : PropertyChangedBase
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

        public GraphReadModel()
        {
            Nodes = new ObservableCollection<NodeVm>();
            Fibers = new ObservableCollection<FiberVm>();
            Rtus = new ObservableCollection<RtuVm>();
            Equipments = new ObservableCollection<EquipmentVm>();
            Traces = new ObservableCollection<TraceVm>();

            IsEquipmentVisible = true;
        }

        #region Node
        private void Apply(NodeAdded evnt)
        {
            var nodeVm = new NodeVm()
            {
                Id = evnt.Id,
                State = FiberState.Ok,
                Type = evnt.IsJustForCurvature ? EquipmentType.Invisible : EquipmentType.Well,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude)
            };
            Nodes.Add(nodeVm);
        }
        #endregion

        #region Fiber
        public void Apply(FiberAdded evnt)
        {
            Fibers.Add(new FiberVm()
            {
                Id = evnt.Id,
                Node1 = Nodes.First(m => m.Id == evnt.Node1),
                Node2 = Nodes.First(m => m.Id == evnt.Node2),
                State = FiberState.NotInTrace
            });
        }
        #endregion

        #region Rtu
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

        public void Apply(RtuUpdated evnt)
        {
            var rtu = Rtus.First(r => r.Id == evnt.Id);
            rtu.Title = evnt.Title;
        }
        #endregion

        #region Equipment
        public void Apply(EquipmentAtGpsLocationAdded evnt)
        {
            var nodeVm = new NodeVm()
            {
                Id = evnt.NodeId,
                State = FiberState.Ok,
                Type = evnt.Type,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude)
            };
            Nodes.Add(nodeVm);

            Equipments.Add(new EquipmentVm() { Id = evnt.Id, Node = nodeVm, Type = evnt.Type });
        }
        #endregion

        #region Trace

        

        #endregion

    }
}