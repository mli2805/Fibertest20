using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;

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

        private void Apply(NodeMoved evnt)
        {
            var nodeVm = Nodes.First(n => n.Id == evnt.Id);
            nodeVm.Position = new PointLatLng(evnt.Latitude, evnt.Longitude);
        }

        private void Apply(NodeUpdated evnt)
        {
            var nodeVm = Nodes.First(n => n.Id == evnt.Id);
            nodeVm.Title = evnt.Title;
            nodeVm.Comment = evnt.Comment;
        }

        private void Apply(NodeRemoved evnt)
        {
            RemoveNodeWithAllHis(evnt.Id);
        }
        private void RemoveNodeWithAllHis(Guid nodeId)
        {
            foreach (var fiberVm in Fibers.Where(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId).ToList())
            {
                Fibers.Remove(fiberVm);
            }

            foreach (var equipmentVm in Equipments.Where(e=>e.Node.Id == nodeId).ToList())
            {
                Equipments.Remove(equipmentVm);
            }

            Nodes.Remove(Nodes.First(n => n.Id == nodeId));
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
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude),
            };
            Nodes.Add(nodeVm);

            var rtuVm = new RtuVm() {Id = evnt.Id, Node = nodeVm};
            Rtus.Add(rtuVm);
        }

        public void Apply(RtuUpdated evnt)
        {
            var rtu = Rtus.First(r => r.Id == evnt.Id);
            rtu.Title = evnt.Title;
            rtu.Node.Title = evnt.Title;
        }

        public void Apply(RtuRemoved evnt)
        {
            var rtuVm = Rtus.First(r => r.Id == evnt.Id);
            Guid nodeId = rtuVm.Node.Id;
            Rtus.Remove(rtuVm);
            RemoveNodeWithAllHis(nodeId);
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

            Equipments.Add(new EquipmentVm() {Id = evnt.Id, Node = nodeVm, Type = evnt.Type});
        }

        #endregion

        #region Trace

        public void Apply(TraceAdded evnt)
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingEventToVm>()).CreateMapper();
            var traceVm = mapper.Map<TraceVm>(evnt);
            Traces.Add(traceVm);
            var fibers = GetFibersByNodes(traceVm.Nodes);
            foreach (var fiberVm in fibers)
            {
                fiberVm.SetState(evnt.Id, traceVm.State);
            }
        }

        public void Apply(TraceStateChanged evnt)
        {
            var traceVm = Traces.First(t => t.Id == evnt.Id);
            var fibers = GetFibersByNodes(traceVm.Nodes);
            foreach (var fiberVm in fibers)
            {
                fiberVm.SetState(evnt.Id, evnt.State);
            }
        }

        private IEnumerable<FiberVm> GetFibersByNodes(List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberByNodes(nodes[i - 1], nodes[i]);
        }

        private FiberVm GetFiberByNodes(Guid node1, Guid node2)
        {
            return Fibers.First(
                f => f.Node1.Id == node1 && f.Node2.Id == node2 ||
                     f.Node1.Id == node2 && f.Node2.Id == node1);
        }

        #endregion
    }
}