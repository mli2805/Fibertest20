using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public static class SceneForEquipmentUpdateRemove
    {
        public static Iit.Fibertest.Graph.Trace SetTraceFromRtuThrouhgAtoB(this SystemUnderTest sut,
            out Guid nodeAId, out Guid equipmentA1Id, out Guid nodeBId, out Guid equipmentB1Id)
        {
            sut.SetNodeWithEquipment(out nodeAId, out equipmentA1Id);
            sut.SetNodeWithEquipment(out nodeBId, out equipmentB1Id);
            var rtuNodeId = sut.SetRtuAndFibers(nodeAId, nodeBId);

            return sut.DefineTrace(nodeBId, rtuNodeId, @"title");
        }

        private static void SetNodeWithEquipment(this SystemUnderTest sut, out Guid nodeA, out Guid eqA)
        {
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeId = sut.ReadModel.Nodes.Last().NodeId;
            nodeA = nodeId;
            eqA = sut.ReadModel.Equipments.Last(e => e.NodeId == nodeId && e.Type == EquipmentType.Closure).EquipmentId;
        }

        private static Guid SetRtuAndFibers(this SystemUnderTest sut, Guid nodeAId, Guid nodeBId)
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation());
            sut.Poller.EventSourcingTick().Wait();
            var rtuNodeId = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = rtuNodeId, NodeId2 = nodeAId }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeAId, NodeId2 = nodeBId }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            return rtuNodeId;
        }
    }
}