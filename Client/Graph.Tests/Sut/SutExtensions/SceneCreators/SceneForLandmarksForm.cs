using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public static class SceneForLandmarksForm
    {
        public static Iit.Fibertest.Graph.Trace SetTraceForLandmarks(this SystemUnderTest sut)
        {
            var rtu = sut.SetInitializedRtu();
            var trace = sut.SetTrace7(rtu.NodeId, @"Trace For Landmarks");
            return trace;
        }

        private static Iit.Fibertest.Graph.Trace SetTrace7(this SystemUnderTest sut, Guid rtuNodeId, string title)
        {
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure, Latitude = 55.074, Longitude = 30.074 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeIdA = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = rtuNodeId, NodeId2 = nodeIdA }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Other, Latitude = 55.112, Longitude = 30.112 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeIdB = sut.ReadModel.Nodes.Last().NodeId;

            sut.FakeWindowManager.RegisterHandler(model => sut.FiberWithNodesAdditionHandler(model, 1, EquipmentType.AdjustmentPoint, Answer.Yes));
            sut.GraphReadModel.GrmFiberWithNodesRequest.AddFiberWithNodes(new RequestAddFiberWithNodes() { Node1 = nodeIdB, Node2 = nodeIdA }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.CableReserve, Latitude = 55.117, Longitude = 30.117 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeIdA = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeIdA, NodeId2 = nodeIdB }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Cross, Latitude = 55.122, Longitude = 30.122 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeIdB = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeIdA, NodeId2 = nodeIdB }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal, Latitude = 55.136, Longitude = 30.136 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var lastNodeId = sut.ReadModel.Nodes.Last().NodeId;

            sut.FakeWindowManager.RegisterHandler(model => sut.FiberWithNodesAdditionHandler(model, 1, EquipmentType.EmptyNode, Answer.Yes));
            sut.GraphReadModel.GrmFiberWithNodesRequest.AddFiberWithNodes(new RequestAddFiberWithNodes(){Node1 = nodeIdB, Node2 = lastNodeId}).Wait();
            sut.Poller.EventSourcingTick().Wait();

            return sut.DefineTrace(lastNodeId, rtuNodeId, title, 6);
        }

    }
}