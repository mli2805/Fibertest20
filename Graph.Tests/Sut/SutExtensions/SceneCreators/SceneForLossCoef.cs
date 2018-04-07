using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public static class SceneForLossCoef
    {
        public static Iit.Fibertest.Graph.Trace SetTraceWithLossCoef(this SystemUnderTest sut)
        {
            var rtu = sut.SetInitializedRtu();
            var trace = sut.SetTrace6(rtu.NodeId, @"Trace with loss coef");
            return trace;
        }

        private static Iit.Fibertest.Graph.Trace SetTrace6(this SystemUnderTest sut, Guid rtuNodeId, string title)
        {
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()
                { Type = EquipmentType.Terminal, Latitude = 55.086, Longitude = 30.086 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var lastNodeId = sut.ReadModel.Nodes.Last().NodeId;

            sut.FakeWindowManager.RegisterHandler(model => sut.FiberWithNodesAdditionHandler(model, 4, EquipmentType.EmptyNode, Answer.Yes));
            sut.GraphReadModel.GrmFiberWithNodesRequest.AddFiberWithNodes(new RequestAddFiberWithNodes(){Node1 = rtuNodeId, Node2 = lastNodeId}).Wait();
            sut.Poller.EventSourcingTick().Wait();

            return sut.DefineTrace(lastNodeId, rtuNodeId, title, 5);
        }

    }
}