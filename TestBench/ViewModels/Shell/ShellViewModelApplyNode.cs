﻿using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private void ApplyToMap(AddNode cmd)
        {
            var nodeVm = new NodeVm()
            {
                Id = cmd.Id,
                State = FiberState.Ok,
                Type = cmd.IsJustForCurvature ? EquipmentType.Invisible : EquipmentType.Well,
                Position = new PointLatLng(cmd.Latitude, cmd.Longitude)
            };
            GraphVm.Nodes.Add(nodeVm);
        }

        private void ApplyToMap(MoveNode cmd)
        {
            var nodeVm = GraphVm.Nodes.Single(n => n.Id == cmd.Id);
            nodeVm.Position = new PointLatLng(cmd.Latitude, cmd.Longitude);
        }

        private void ApplyToMap(UpdateNode cmd)
        {
            var nodeVm = GraphVm.Nodes.Single(n => n.Id == cmd.Id);
            nodeVm.Title = cmd.Title;
            nodeVm.Comment = cmd.Comment;
        }

        private RemoveNode PrepareCommand(RequestRemoveNode request)
        {
            if (GraphVm.Traces.Any(t => t.Nodes.Last() == request.Id))
                return null; // It's prohibited to remove last node from trace
            if (GraphVm.Traces.Any(t => t.Nodes.Contains(request.Id) && t.HasBase))
                return null; // It's prohibited to remove any node from trace with base ref

            var dictionary = GraphVm.Traces.Where(t => t.Nodes.Contains(request.Id)).ToDictionary(trace => trace.Id, trace => Guid.NewGuid());
            return new RemoveNode { Id = request.Id, TraceFiberPairForDetour = dictionary };

        }

        private UpdateNode PrepareCommand(UpdateNode request)
        {
            var vm = new NodeUpdateViewModel(request.Id, GraphVm, _windowManager);
            vm.PropertyChanged += Vm_PropertyChanged;
            _windowManager.ShowDialog(vm);
            return vm.Command != null ? (UpdateNode)vm.Command : null;
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Command")
                return;
            var vm = (NodeUpdateViewModel)sender;
            
            if (vm.Command is RemoveEquipment)
                ApplyToMap((RemoveEquipment)vm.Command); 
            //TODO how to send to aggregate, implement other types of command   
        }

        private void ApplyToMap(RemoveNode cmd)
        {
            var fiberVms = GraphVm.Fibers.Where(e => e.Node1.Id == cmd.Id || e.Node2.Id == cmd.Id).ToList();
            foreach (var fiberVm in fiberVms)
            {
                GraphVm.Fibers.Remove(fiberVm);
            }
            GraphVm.Nodes.Remove(GraphVm.Nodes.Single(n => n.Id == cmd.Id));
        }

        #region AddNodeIntoFiber
        /// <summary>
        /// Attention! Mind the difference with Fibertest 1.5
        /// This command for add node (well) only!
        /// Equipment should be added by separate command!
        /// </summary>
        private AddNodeIntoFiber PrepareCommand(RequestAddNodeIntoFiber request)
        {
            if (IsFiberContainedInAnyTraceWithBase(request.FiberId))
            {
                _windowManager.ShowDialog(new NotificationViewModel("", "It's impossible to change trace with base reflectogram"));
                return null;
            }

            return new AddNodeIntoFiber()
            {
                Id = Guid.NewGuid(),
                Position = GetFiberCenter(request.FiberId),
                FiberId = request.FiberId,
                NewFiberId1 = Guid.NewGuid(),
                NewFiberId2 = Guid.NewGuid()
            };
        }

        private bool IsFiberContainedInAnyTraceWithBase(Guid fiberId)
        {
            var tracesWithBase = GraphVm.Traces.Where(t => t.HasBase);
            var fiber = GraphVm.Fibers.Single(f => f.Id == fiberId);
            foreach (var trace in tracesWithBase)
            {
                if (GetFiberIndexInTrace(trace, fiber) != -1)
                    return true;
            }
            return false;
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

        private void ApplyToMap(AddNodeIntoFiber cmd)
        {

            GraphVm.Nodes.Add(new NodeVm() { Id = cmd.Id, Position = new PointLatLng(cmd.Position.Latitude, cmd.Position.Longitude) });
            AddTwoFibersToNewNode(cmd);
            FixTracesWhichContainedOldFiber(cmd);
            GraphVm.Fibers.Remove(GraphVm.Fibers.Single(f => f.Id == cmd.FiberId));
        }

        private void AddTwoFibersToNewNode(AddNodeIntoFiber cmd)
        {
            var nodeVm = GraphVm.Nodes.First(n => n.Id == cmd.Id);
            NodeVm node1 = GraphVm.Fibers.Single(f => f.Id == cmd.FiberId).Node1;
            NodeVm node2 = GraphVm.Fibers.Single(f => f.Id == cmd.FiberId).Node2;
            GraphVm.Fibers.Add(new FiberVm() { Id = cmd.NewFiberId1, Node1 = node1, Node2 = nodeVm });
            GraphVm.Fibers.Add(new FiberVm() { Id = cmd.NewFiberId2, Node1 = nodeVm, Node2 = node2 });
        }
        private void FixTracesWhichContainedOldFiber(AddNodeIntoFiber cmd)
        {
            foreach (var trace in GraphVm.Traces)
            {
                int idx;
                while ((idx = GetFiberIndexInTrace(trace, GraphVm.Fibers.Single(f => f.Id == cmd.FiberId))) != -1)
                {
                    trace.Nodes.Insert(idx + 1, cmd.Id); // GPS location добавляется во все трассы
                }
            }
        }

        private GpsLocation GetFiberCenter(Guid fiberId)
        {
            var fiber = GraphVm.Fibers.Single(f => f.Id == fiberId);
            var node1 = GraphVm.Nodes.Single(n => n.Id == fiber.Node1.Id);
            var node2 = GraphVm.Nodes.Single(n => n.Id == fiber.Node2.Id);
            return new GpsLocation() { Latitude = (node1.Position.Lat + node2.Position.Lat) / 2, Longitude = (node1.Position.Lng + node2.Position.Lng) / 2 };
        }
        #endregion

    }

}
