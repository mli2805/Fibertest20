using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;

namespace Graph.Tests
{
    public class SystemUnderTest
    {
        public Aggregate Aggregate { get; } = new Aggregate();
        public ReadModel ReadModel { get; } = new ReadModel();
        public ClientPoller Poller { get; }
        public int CurrentEventNumber => Poller.CurrentEventNumber;
        public MapViewModel MapVm { get; }
        public FakeWindowManager FakeWindowManager { get; }

        public SystemUnderTest()
        {
            Poller = new ClientPoller(Aggregate.WriteModel.Db, new List<object> { ReadModel });
            FakeWindowManager = new FakeWindowManager();
            MapVm = new MapViewModel(Aggregate, ReadModel, FakeWindowManager);
        }

        public void CreateTraceRtuEmptyTerminal()
        {
            var equipments = new List<Guid>();
            MapVm.AddRtuAtGpsLocation();
            Poller.Tick();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;
            equipments.Add(ReadModel.Rtus.Last().Id);
            MapVm.AddNode();
            MapVm.AddEquipmentAtGpsLocation(EquipmentType.Terminal);
            Poller.Tick();
            var firstNodeId = ReadModel.Nodes[1].Id;
            var secondNodeId = ReadModel.Nodes.Last().Id;
            equipments.Add(Guid.Empty);
            equipments.Add(ReadModel.Equipments.Last().Id);
            MapVm.AddFiber(nodeForRtuId, firstNodeId);
            MapVm.AddFiber(firstNodeId, secondNodeId);
            Poller.Tick();
            var addTraceViewModel = new AddTraceViewModel(FakeWindowManager, ReadModel, Aggregate, new List<Guid>() { nodeForRtuId, firstNodeId, secondNodeId }, equipments);
            addTraceViewModel.Save();
            Poller.Tick();
        }

        public void CreatePositionForAddNodeIntoFiberTest()
        {
            MapVm.AddRtuAtGpsLocation();
            Poller.Tick();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;
            MapVm.AddNode();
            Poller.Tick();
            var a1 = ReadModel.Nodes.Last().Id;
            MapVm.AddNode();
            Poller.Tick();
            var b1 = ReadModel.Nodes.Last().Id;
            MapVm.AddFiber(a1, b1); // fiber for insertion
            Poller.Tick();

            MapVm.AddEquipmentAtGpsLocation(EquipmentType.Terminal);
            Poller.Tick();
            var a2 = ReadModel.Nodes.Last().Id;
            MapVm.AddEquipmentAtGpsLocation(EquipmentType.Terminal);
            Poller.Tick();
            var b2 = ReadModel.Nodes.Last().Id;
            MapVm.AddEquipmentAtGpsLocation(EquipmentType.Terminal);
            Poller.Tick();
            var c2 = ReadModel.Nodes.Last().Id;
            MapVm.AddEquipmentAtGpsLocation(EquipmentType.Terminal);
            Poller.Tick();
            var d2 = ReadModel.Nodes.Last().Id;

            MapVm.AddFiber(nodeForRtuId, a1);
            MapVm.AddFiber(nodeForRtuId, b1);
            MapVm.AddFiber(a1, b2);
            MapVm.AddFiber(a1, c2);
            MapVm.AddFiber(nodeForRtuId, d2);
            MapVm.AddFiber(b1, a2);
            Poller.Tick();

            var equipments = new List<Guid> {ReadModel.Rtus.Last().Id, Guid.Empty, Guid.Empty, ReadModel.Equipments.Single(e=>e.NodeId == a2).Id};
            var addTraceViewModel = new AddTraceViewModel(FakeWindowManager, ReadModel, Aggregate, new List<Guid>() { nodeForRtuId, a1, b1, a2 }, equipments);
            addTraceViewModel.Save();
            Poller.Tick();

            equipments = new List<Guid> {ReadModel.Rtus.Last().Id, Guid.Empty, Guid.Empty, ReadModel.Equipments.Single(e=>e.NodeId == b2).Id};
            addTraceViewModel = new AddTraceViewModel(FakeWindowManager, ReadModel, Aggregate, new List<Guid>() { nodeForRtuId, b1, a1, b2 }, equipments);
            addTraceViewModel.Save();
            Poller.Tick();

            equipments = new List<Guid> { ReadModel.Rtus.Last().Id, Guid.Empty, ReadModel.Equipments.Single(e => e.NodeId == c2).Id };
            addTraceViewModel = new AddTraceViewModel(FakeWindowManager, ReadModel, Aggregate, new List<Guid>() { nodeForRtuId, a1, c2 }, equipments);
            addTraceViewModel.Save();
            Poller.Tick();

            equipments = new List<Guid> { ReadModel.Rtus.Last().Id, ReadModel.Equipments.Single(e => e.NodeId == d2).Id };
            addTraceViewModel = new AddTraceViewModel(FakeWindowManager, ReadModel, Aggregate, new List<Guid>() { nodeForRtuId, d2 }, equipments);
            addTraceViewModel.Save();
            Poller.Tick();
        }


        public void CreateFieldForPathFinderTest(Guid startId, Guid finishId)
        {
            var a0 = ReadModel.Nodes.Single(n => n.Id == startId);

            var b0 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "b0" };
            ReadModel.Nodes.Add(b0);
            var b1 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "b1" };
            ReadModel.Nodes.Add(b1);
            var b2 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "b2" };
            ReadModel.Nodes.Add(b2);
            var c0 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "c0" };
            ReadModel.Nodes.Add(c0);
            var c1 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "c1" };
            ReadModel.Nodes.Add(c1);
            var c2 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "c2" };
            ReadModel.Nodes.Add(c2);
            var d0 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "d0" };
            ReadModel.Nodes.Add(d0);
            var d1 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "d1" };
            ReadModel.Nodes.Add(d1);
            var d2 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "d2" };
            ReadModel.Nodes.Add(d2);
            var e0 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "e0" };
            ReadModel.Nodes.Add(e0);
            var e1 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "e1" };
            ReadModel.Nodes.Add(e1);
            var e2 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "e2" };
            ReadModel.Nodes.Add(e2);

            var nn = ReadModel.Nodes.Single(n => n.Id == finishId);

            var zz = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "zz" };
            ReadModel.Nodes.Add(zz);
            var z2 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "z2" };
            ReadModel.Nodes.Add(z2);


            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = a0.Id, Node2 = b0.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = a0.Id, Node2 = b1.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = a0.Id, Node2 = b2.Id });

            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c0.Id, Node2 = b0.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c1.Id, Node2 = b1.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c2.Id, Node2 = b2.Id });

            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c0.Id, Node2 = d0.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c1.Id, Node2 = d1.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c2.Id, Node2 = d2.Id });

            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = e0.Id, Node2 = d0.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = e1.Id, Node2 = d1.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = e2.Id, Node2 = d2.Id });

            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = e2.Id, Node2 = nn.Id });

            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = zz.Id, Node2 = z2.Id });
        }

    }
}