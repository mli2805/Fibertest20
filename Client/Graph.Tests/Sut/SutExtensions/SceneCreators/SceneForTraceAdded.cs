using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public static class SceneForTraceAdded
    {
        public static void CreateFieldForPathFinderTest(this SystemUnderTest sut, out Guid startId, out Guid finishId, out Guid wrongNodeId, out Guid wrongNodeWithEqId)
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            sut.Poller.EventSourcingTick().Wait();
            startId = sut.ReadModel.Rtus.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var b0 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var b1 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var b2 = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var c0 = sut.ReadModel.Nodes.Last().NodeId; wrongNodeId = c0;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var c1 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure }).Wait(); var c2 = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var d0 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var d1 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure }).Wait(); var d2 = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var e0 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var e1 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var e2 = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait(); finishId = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var zz = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait(); var z2 = sut.ReadModel.Nodes.Last().NodeId;
            wrongNodeWithEqId = z2;


            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = Guid.NewGuid(), NodeId1 = startId, NodeId2 = b0 }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = startId, NodeId2 = b1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = startId, NodeId2 = b2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = c0, NodeId2 = b0 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = c1, NodeId2 = b1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = c2, NodeId2 = b2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = c0, NodeId2 = d0 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = c1, NodeId2 = d1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = c2, NodeId2 = d2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = e0, NodeId2 = d0 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = e1, NodeId2 = d1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = e2, NodeId2 = d2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = e2, NodeId2 = finishId }).Wait();
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = new Guid(), NodeId1 = zz, NodeId2 = z2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
        }

    }
}