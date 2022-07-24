using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public static class SceneForAddNodeIntoFiber
    {
        public static void CreatePositionForAddNodeIntoFiberTest(this SystemUnderTest sut, out Iit.Fibertest.Graph.Fiber fiberForInsertion,
            out Iit.Fibertest.Graph.Trace traceForInsertionId)
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            sut.Poller.EventSourcingTick().Wait();
            var nodeForRtuId = sut.ReadModel.Rtus.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var a1 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var b1 = sut.ReadModel.Nodes.Last().NodeId;
            // fiber for insertion
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = a1, NodeId2 = b1 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            fiberForInsertion = sut.ReadModel.Fibers.Last();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var a2 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var b2 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var c2 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var d2 = sut.ReadModel.Nodes.Last().NodeId;
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeForRtuId, NodeId2 = a1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = a1, NodeId2 = a2 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = b1, NodeId2 = b2 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = b1, NodeId2 = c2 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeForRtuId, NodeId2 = d2 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = b1, NodeId2 = a2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            
            traceForInsertionId = sut.DefineTrace(a2, nodeForRtuId, @"some title", 3);
            sut.DefineTrace(b2, nodeForRtuId, @"some title", 3);
            sut.DefineTrace(c2, nodeForRtuId, @"some title", 3);
            sut.DefineTrace(d2, nodeForRtuId);
        }

    }
}