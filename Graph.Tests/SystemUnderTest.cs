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

        public void CreateTrace()
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
            var addTraceViewModel = new AddTraceViewModel(ReadModel, Aggregate, new List<Guid>() { nodeForRtuId, firstNodeId, secondNodeId }, equipments);
            addTraceViewModel.Save();
            Poller.Tick();
        }
    }
}