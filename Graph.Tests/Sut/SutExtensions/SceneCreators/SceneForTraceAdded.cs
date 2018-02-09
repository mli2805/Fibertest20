using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public static class SceneForTraceAdded
    {
        public static void CreateFieldForPathFinderTest(this SystemUnderTest sut, out Guid startId, out Guid finishId, out Guid wrongNodeId, out Guid wrongNodeWithEqId)
        {
            sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            startId = sut.ReadModel.Rtus.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var b0 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var b1 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var b2 = sut.ReadModel.Nodes.Last().Id;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var c0 = sut.ReadModel.Nodes.Last().Id; wrongNodeId = c0;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var c1 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure }).Wait(); var c2 = sut.ReadModel.Nodes.Last().Id;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var d0 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var d1 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure }).Wait(); var d2 = sut.ReadModel.Nodes.Last().Id;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var e0 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var e1 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var e2 = sut.ReadModel.Nodes.Last().Id;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait(); finishId = sut.ReadModel.Nodes.Last().Id;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait(); var zz = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait(); var z2 = sut.ReadModel.Nodes.Last().Id;
            wrongNodeWithEqId = z2;


            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = Guid.NewGuid(), Node1 = startId, Node2 = b0 }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = startId, Node2 = b1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = startId, Node2 = b2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = c0, Node2 = b0 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = c1, Node2 = b1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = c2, Node2 = b2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = c0, Node2 = d0 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = c1, Node2 = d1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = c2, Node2 = d2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = e0, Node2 = d0 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = e1, Node2 = d1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = e2, Node2 = d2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = e2, Node2 = finishId }).Wait();
            
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Id = new Guid(), Node1 = zz, Node2 = z2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
        }

    }
}