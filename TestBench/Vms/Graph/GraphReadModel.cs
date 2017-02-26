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
            Nodes.Add(new NodeVm()
            {
                Id = evnt.Id,
                State = FiberState.Ok,
                Type = evnt.IsJustForCurvature ? EquipmentType.Invisible : EquipmentType.Well,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude)
            });
        }

        private void Apply(NodeIntoFiberAdded evnt)
        {
            Nodes.Add(new NodeVm()
            {
                Id=evnt.Id,
                Position = new PointLatLng(evnt.Position.Latitude, evnt.Position.Longitude),
                State = FiberState.Ok,
                Type = EquipmentType.Well
            });
            var fiberForDeletion = Fibers.First(f => f.Id == evnt.FiberId);
            AddTwoFibersToNewNode(evnt, fiberForDeletion);
            FixTracesWhichContainedOldFiber(evnt);
            Fibers.Remove(fiberForDeletion);
        }

        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in Traces)
            {
                int idx;
                while ((idx = GetFiberIndexInTrace(trace, Fibers.First(f => f.Id == e.FiberId))) != -1)
                {
                    trace.Nodes.Insert(idx + 1, e.Id);
                }
            }
        }
        private int GetFiberIndexInTrace(TraceVm trace, FiberVm fiber)
        {
            var idxInTrace1 = trace.Nodes.IndexOf(fiber.Node1.Id);
            if (idxInTrace1 == -1)
                return -1;
            var idxInTrace2 = trace.Nodes.IndexOf(fiber.Node2.Id);
            if (idxInTrace2 == -1)
                return -1;
            if (idxInTrace2 - idxInTrace1 == 1)
                return idxInTrace1;
            if (idxInTrace1 - idxInTrace2 == 1)
                return idxInTrace2;
            return -1;
        }

        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e, FiberVm oldFiberVm)
        {
            NodeVm node1 = Fibers.First(f => f.Id == e.FiberId).Node1;
            NodeVm node2 = Fibers.First(f => f.Id == e.FiberId).Node2;

            Fibers.Add(new FiberVm() { Id = e.NewFiberId1, Node1 = node1, Node2 = Nodes.First(n=>n.Id == e.Id), States = oldFiberVm.States });
            Fibers.Add(new FiberVm() { Id = e.NewFiberId2, Node1 = Nodes.First(n => n.Id == e.Id), Node2 = node2, States = oldFiberVm.States });
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
                Fibers.Remove(fiberVm);
            foreach (var equipmentVm in Equipments.Where(e => e.Node.Id == nodeId).ToList())
                Equipments.Remove(equipmentVm);

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

        public void Apply(FiberRemoved evnt)
        {
            Fibers.Remove(Fibers.First(f => f.Id == evnt.Id));
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

        public void Apply(EquipmentIntoNodeAdded evnt)
        {
            var nodeVm = Nodes.First(n => n.Id == evnt.NodeId);
            nodeVm.Type = evnt.Type;
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