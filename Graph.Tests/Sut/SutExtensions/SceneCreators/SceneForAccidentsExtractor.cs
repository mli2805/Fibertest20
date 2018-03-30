using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public static class SceneForAccidentsExtractor
    {
        public static Iit.Fibertest.Graph.Trace SetTraceWithAccidentInOldNode(this SystemUnderTest sut)
        {
            var rtu = sut.SetInitializedRtu();
            var trace = sut.SetTrace(rtu.NodeId, @"Trace with accident in existing node");
            var traceLeaf = (TraceLeaf)sut.TreeOfRtuModel.Tree.GetById(trace.Id);
            sut.AssignBaseRef(traceLeaf, SystemUnderTest.Base1550Lm4RealplaceYesRough, SystemUnderTest.Base1550Lm4RealplaceYesRough, null, Answer.Yes);
            return trace;
        }

        public static TraceLeaf Attach(this SystemUnderTest sut, Iit.Fibertest.Graph.Trace trace, int portNumber)
        {
            var rtuLeaf = (RtuLeaf)sut.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(trace.RtuId);
            sut.AttachTraceTo(trace.Id, rtuLeaf, portNumber, Answer.Yes);
            return (TraceLeaf)sut.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(trace.Id);
        }

        public static Iit.Fibertest.Graph.Trace SetTraceWithAccidentBetweenNodes(this SystemUnderTest sut)
        {
            var rtu = sut.SetInitializedRtu();
            var trace = sut.SetTraceWithAccidentBetweenNodes(rtu.NodeId, @"Trace with accident between nodes");
            var traceLeaf = (TraceLeaf)sut.TreeOfRtuModel.Tree.GetById(trace.Id);
            sut.AssignBaseRef(traceLeaf, SystemUnderTest.Base1550Lm5FakeYesRough, SystemUnderTest.Base1550Lm5FakeYesRough, null, Answer.Yes);
            return trace;
        }

        private static Iit.Fibertest.Graph.Trace SetTraceWithAccidentBetweenNodes(this SystemUnderTest sut, Guid rtuNodeId, string title)
        {
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure, Latitude = 55.074, Longitude = 30.074 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeIdA = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = rtuNodeId, NodeId2 = nodeIdA }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure, Latitude = 55.112, Longitude = 30.112 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeIdB = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeIdA, NodeId2 = nodeIdB }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Cross, Latitude = 55.117, Longitude = 30.117 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeIdA = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeIdA, NodeId2 = nodeIdB }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal, Latitude = 55.122, Longitude = 30.122 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeIdB = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeIdA, NodeId2 = nodeIdB }).Wait();

            sut.Poller.EventSourcingTick().Wait();

            return sut.DefineTrace(nodeIdB, rtuNodeId, title, 4);
        }

        public static void AssertTraceFibersState(this SystemUnderTest sut, Iit.Fibertest.Graph.Trace trace)
        {
            var fibers = sut.ReadModel.GetTraceFibers(trace).ToList();
            foreach (var fiber in fibers)
            {
                fiber.States.Contains(new KeyValuePair<Guid, FiberState>(trace.Id, trace.State)).Should().Be(true);
                var fiberVm = sut.GraphReadModel.Data.Fibers.First(f => f.Id == fiber.FiberId);
                fiberVm.States.Contains(new KeyValuePair<Guid, FiberState>(trace.Id, trace.State)).Should().Be(true);
            }
        }
    }
}