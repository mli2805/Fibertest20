using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public static class SceneForTraceCleanRemove
    {
        public static void CreateTwoTraces(this SystemUnderTest sut, out Guid traceId1, out Guid traceId2)
        {
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeForRtuId = sut.ReadModel.Rtus.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var a1 = sut.ReadModel.Nodes.Last().Id;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var a2 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var b2 = sut.ReadModel.Nodes.Last().Id;

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = nodeForRtuId, Node2 = a1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = a1, Node2 = a2 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = a1, Node2 = b2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            traceId1 = sut.DefineTrace(a2, nodeForRtuId).Id;
            traceId2 = sut.DefineTrace(b2, nodeForRtuId).Id;
        }
    }
}