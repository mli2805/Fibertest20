using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public static class SceneForRtuRemoved
    {
        public static Iit.Fibertest.Graph.Rtu CreateRtuA(this SystemUnderTest sut)
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation());
            sut.Poller.EventSourcingTick().Wait();
            return sut.ReadModel.Rtus.Last();
        }

        public static Guid[] CreateOneRtuAndFewNodesAndFibers(this SystemUnderTest sut, Guid rtuANodeId)
        {
            var result = new Guid[3];

            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation());
            sut.Poller.EventSourcingTick().Wait();
            result[0] = sut.ReadModel.Rtus.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var node1Id = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            result[1] = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            result[2] = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = rtuANodeId, NodeId2 = node1Id }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = node1Id, NodeId2 = result[1] }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = result[1], NodeId2 = result[2] }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = result[2], NodeId2 = result[0] }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            return result;
        }

       
    }
}