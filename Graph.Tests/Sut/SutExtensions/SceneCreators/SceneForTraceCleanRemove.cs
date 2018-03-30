using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public static class SceneForTraceCleanRemove
    {
        public static void CreateTwoTraces(this SystemUnderTest sut, out Guid traceId1, out Guid traceId2)
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            sut.Poller.EventSourcingTick().Wait();
            var nodeForRtuId = sut.ReadModel.Rtus.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var a1 = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var a2 = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var b2 = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeForRtuId, NodeId2 = a1 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = a1, NodeId2 = a2 }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = a1, NodeId2 = b2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            traceId1 = sut.DefineTrace(a2, nodeForRtuId).TraceId;
            traceId2 = sut.DefineTrace(b2, nodeForRtuId).TraceId;
        }
    }
}