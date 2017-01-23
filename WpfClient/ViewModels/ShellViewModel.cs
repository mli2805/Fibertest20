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
            var updateNodeViewModel = new NodeUpdateViewModel(node.Id, _readModel, _aggregate);
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
            var updateFiberViewModel = new FiberUpdateViewModel(fiber.Id, _readModel, _aggregate);
            windowManager.ShowDialog(updateFiberViewModel);

        }

        public void LaunchEquipmentView()
        {
            var node = new Node { Id = Guid.NewGuid() };
            _readModel.Nodes.Add(node);
            var equipment = new Equipment {Id = Guid.NewGuid(), NodeId = node.Id, Type = EquipmentType.Cross};

            var windowManager = IoC.Get<IWindowManager>();
            var equipmentViewModel = new EquipmentViewModel(windowManager, Guid.Empty, equipment.Id, null, _aggregate);
            windowManager.ShowDialog(equipmentViewModel);

        }

        public void LaunchAssignBaseRefs()
        {
            var trace = _readModel.Traces.First();

            var windowManager = IoC.Get<IWindowManager>();
            var addEquipmentViewModel = new BaseRefsAssignViewModel(trace.Id, _readModel, _aggregate);
            windowManager.ShowDialog(addEquipmentViewModel);
        }

        public void FakeTraceDefine()
        {
            var mapVm = new MapViewModel(_aggregate, _readModel, IoC.Get<IWindowManager>());

            var rtuNodeId = Guid.NewGuid();
            _readModel.Nodes.Add(new Node {Id = rtuNodeId, Title = "Первый РТУ" });
            _readModel.Rtus.Add(new Rtu {Id = Guid.NewGuid(), NodeId = rtuNodeId, Title = "Первый РТУ"});

            var lastNodeId = Guid.NewGuid();
            _readModel.Nodes.Add(new Node { Id = lastNodeId , Title = "оконечный кросс" });
            _readModel.Equipments.Add(new Equipment { Id = Guid.NewGuid(), NodeId = lastNodeId, Type = EquipmentType.Terminal, Title = "оконечный кросс"});

            CreateFieldForPathFinderTest(_readModel, rtuNodeId, lastNodeId);

            mapVm.DefineTraceClick(rtuNodeId, lastNodeId);
        }
        public void CreateFieldForPathFinderTest(ReadModel readModel, Guid startId, Guid finishId)
        {

            var b0 = new Node { Id = Guid.NewGuid(), Title = "b0" };
            readModel.Nodes.Add(b0);
            var b1 = new Node { Id = Guid.NewGuid(), Title = "b1" };
            readModel.Nodes.Add(b1);
            var b2 = new Node { Id = Guid.NewGuid(), Title = "b2" };
            readModel.Nodes.Add(b2);
            var c0 = new Node { Id = Guid.NewGuid(), Title = "c0" };
            readModel.Nodes.Add(c0);
            var c1 = new Node { Id = Guid.NewGuid(), Title = "c1" };
            readModel.Nodes.Add(c1);
            var c2 = new Node { Id = Guid.NewGuid(), Title = "c2" };
            readModel.Nodes.Add(c2);
            var d0 = new Node { Id = Guid.NewGuid(), Title = "d0" };
            readModel.Nodes.Add(d0);
            var d1 = new Node { Id = Guid.NewGuid(), Title = "d1" };
            readModel.Nodes.Add(d1);
            var d2 = new Node { Id = Guid.NewGuid(), Title = "d2" };
            readModel.Nodes.Add(d2);
            var e0 = new Node { Id = Guid.NewGuid(), Title = "e0" };
            readModel.Nodes.Add(e0);
            var e1 = new Node { Id = Guid.NewGuid(), Title = "e1" };
            readModel.Nodes.Add(e1);
            var e2 = new Node { Id = Guid.NewGuid(), Title = "e2" };
            readModel.Nodes.Add(e2);

            var zz = new Node { Id = Guid.NewGuid(), Title = "zz" };
            readModel.Nodes.Add(zz);
            var z2 = new Node { Id = Guid.NewGuid(), Title = "z2" };
            readModel.Nodes.Add(z2);


            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = startId, Node2 = b0.Id });
            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = startId, Node2 = b1.Id });
            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = startId, Node2 = b2.Id });

            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = c0.Id, Node2 = b0.Id });
            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = c1.Id, Node2 = b1.Id });
            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = c2.Id, Node2 = b2.Id });

            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = c0.Id, Node2 = d0.Id });
            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = c1.Id, Node2 = d1.Id });
            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = c2.Id, Node2 = d2.Id });

            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = e0.Id, Node2 = d0.Id });
            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = e1.Id, Node2 = d1.Id });
            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = e2.Id, Node2 = d2.Id });

            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = e2.Id, Node2 = finishId });

            readModel.Fibers.Add(new Fiber { Id = new Guid(), Node1 = zz.Id, Node2 = z2.Id });

            readModel.Equipments.Add(new Equipment() {Id = Guid.NewGuid(), NodeId = d2.Id, Type = EquipmentType.Cross, Title = "C1"});
            readModel.Equipments.Add(new Equipment() {Id = Guid.NewGuid(), NodeId = d2.Id, Type = EquipmentType.Sleeve, Title = "M1"});

            readModel.Equipments.Add(new Equipment() {Id = Guid.NewGuid(), NodeId = c2.Id, Type = EquipmentType.Other, Title = "O1"});
        }
    }
}