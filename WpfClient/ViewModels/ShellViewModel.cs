using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        private readonly Aggregate _aggregate;
        private readonly ReadModel _readModel;
        public ShellViewModel()
        {
            _aggregate = new Aggregate();
            _readModel = new ReadModel();
        }

        public void LaunchUpdateNodeView()
        {
            var node = new Node {Id = Guid.NewGuid()};
            _readModel.Nodes.Add(node);

            var windowManager = IoC.Get<IWindowManager>();
            var updateNodeViewModel = new UpdateNodeViewModel(node.Id, _readModel, _aggregate);
            windowManager.ShowDialog(updateNodeViewModel);

        }

        public void LaunchUpdateFiberView()
        {
            var node1 = new Node { Id = Guid.NewGuid() };
            _readModel.Nodes.Add(node1);
            var node2 = new Node { Id = Guid.NewGuid() };
            _readModel.Nodes.Add(node2);
            var fiber = new Fiber {Id = Guid.NewGuid(), Node1 = node1.Id, Node2 = node2.Id};
            _readModel.Fibers.Add(fiber);

            var windowManager = IoC.Get<IWindowManager>();
            var updateFiberViewModel = new UpdateFiberViewModel(fiber.Id, _readModel, _aggregate);
            windowManager.ShowDialog(updateFiberViewModel);

        }

        public void LaunchAssignBaseRefs()
        {
            var trace = _readModel.Traces.First();

            var windowManager = IoC.Get<IWindowManager>();
            var addEquipmentViewModel = new AssignBaseRefsViewModel(trace.Id, _readModel, _aggregate);
            windowManager.ShowDialog(addEquipmentViewModel);
        }

    }
}