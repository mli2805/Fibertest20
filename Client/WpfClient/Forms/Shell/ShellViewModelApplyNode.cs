using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public partial class ShellViewModel
    {
        private RemoveNode PrepareCommand(RequestRemoveNode request)
        {
            if (GraphReadModel.Traces.Any(t => t.Nodes.Last() == request.Id))
                return null; // It's prohibited to remove last node from trace
            if (GraphReadModel.Traces.Any(t => t.Nodes.Contains(request.Id) && t.HasBase))
                return null; // It's prohibited to remove any node from trace with base ref

            var dictionary = GraphReadModel.Traces.Where(t => t.Nodes.Contains(request.Id)).ToDictionary(trace => trace.Id, trace => Guid.NewGuid());
            return new RemoveNode { Id = request.Id, TraceWithNewFiberForDetourRemovedNode = dictionary };
        }

        private UpdateNode PrepareCommand(UpdateNode request)
        {
            var vm = new NodeUpdateViewModel(request.Id, ReadModel, _windowManager, C2DWcfManager);
            vm.PropertyChanged += Vm_PropertyChanged;
            _windowManager.ShowDialog(vm);
            return (UpdateNode)vm.Command;
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Command")
                return;
            var vm = (NodeUpdateViewModel)sender;

            if (vm.Command is AddEquipmentIntoNode)
                C2DWcfManager.SendCommandAsObj((AddEquipmentIntoNode)vm.Command);


            if (vm.Command is UpdateEquipment)
                C2DWcfManager.SendCommandAsObj((UpdateEquipment)vm.Command);

            if (vm.Command is RemoveEquipment)
                ComplyWithRequest((RemoveEquipment)vm.Command).Wait();
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
                _windowManager.ShowDialog(new NotificationViewModel("", Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram));
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
            var fiber = ReadModel.Fibers.First(f => f.Id == fiberId);
            return ReadModel.Traces.Where(t => t.HasAnyBaseRef).ToList().Any(trace => Topo.GetFiberIndexInTrace(trace, fiber) != -1);
        }

        private PointLatLng GetFiberCenter(Guid fiberId)
        {
            var fiber = GraphReadModel.Fibers.Single(f => f.Id == fiberId);
            var node1 = GraphReadModel.Nodes.Single(n => n.Id == fiber.Node1.Id);
            var node2 = GraphReadModel.Nodes.Single(n => n.Id == fiber.Node2.Id);
            return new PointLatLng() { Lat = (node1.Position.Lat + node2.Position.Lat) / 2, Lng = (node1.Position.Lng + node2.Position.Lng) / 2 };
        }
        #endregion
    }
}
